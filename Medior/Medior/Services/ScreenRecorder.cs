using Medior.BaseTypes;
using Medior.Extensions;
using Medior.Models;
using Medior.Utilities;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;

namespace Medior.Services
{
    public interface IScreenRecorder 
    {
        Task<Result> CaptureVideo(
            DisplayInfo display,
            int frameRate,
            Stream destinationStream, 
            CancellationToken cancellationToken);
    }

    public class ScreenRecorder : IScreenRecorder
    {
        private readonly IScreenCapturer _screenCapturer;

        public ScreenRecorder(IScreenCapturer screenCapturer)
        {
            _screenCapturer = screenCapturer;
        }
        public async Task<Result> CaptureVideo(
            DisplayInfo display,
            int frameRate,
            Stream destinationStream, 
            CancellationToken cancellationToken)
        {
            try
            {

                var captureArea = new Rectangle(Point.Empty, display.Bounds.Size);

                var evenWidth = captureArea.Width % 2 == 0 ? (uint)captureArea.Width : (uint)captureArea.Width + 1;
                var evenHeight = captureArea.Height % 2 == 0 ? (uint)captureArea.Height : (uint)captureArea.Height + 1;

                var size = captureArea.Width * 4 * captureArea.Height;

                var tempBuffer = new byte[size];

                var sourceVideoProperties = VideoEncodingProperties.CreateUncompressed(
                    MediaEncodingSubtypes.Argb32,
                    evenWidth,
                    evenHeight);

                var videoDescriptor = new VideoStreamDescriptor(sourceVideoProperties);

                var mediaStreamSource = new MediaStreamSource(videoDescriptor)
                {
                    BufferTime = TimeSpan.Zero
                };


                Bitmap? currentFrame = null;
                var frameLock = new SemaphoreSlim(1, 1);
                var frameSignal = new AutoResetEvent(false);

                _screenCapturer.OnFrameArrived += (sender, newFrame) =>
                {
                    try
                    {
                        frameLock.Wait();
                        newFrame.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        currentFrame?.Dispose();
                        // Screen capturer will dispose of the bitmap after invoking
                        // handlers, so we need to clone it.
                        currentFrame = (Bitmap)newFrame.Clone();
                        frameSignal.Set();
                    }
                    finally
                    {
                        frameLock.Release();
                    }
                };

                var stopwatch = Stopwatch.StartNew();

                mediaStreamSource.Starting += (sender, args) =>
                {
                    args.Request.SetActualStartPosition(stopwatch.Elapsed);
                };

                mediaStreamSource.SampleRequested += (sender, args) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        args.Request.Sample = null;
                        return;
                    }

                    try
                    {
                        // The transcoder will stop if don't return a bitmap for
                        // each request.  So we'll wait until one is available.
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            if (frameSignal.WaitOne(1000))
                            {
                                break;
                            }
                        }

                        frameLock.Wait();

                        if (currentFrame is null)
                        {
                            return;
                        }

                        var bd = currentFrame.LockBits(new Rectangle(Point.Empty, captureArea.Size), ImageLockMode.ReadOnly, currentFrame.PixelFormat);
                        Marshal.Copy(bd.Scan0, tempBuffer, 0, size);
                        args.Request.Sample = MediaStreamSample.CreateFromBuffer(tempBuffer.AsBuffer(), stopwatch.Elapsed);
                        currentFrame.UnlockBits(bd);
                    }
                    finally
                    {
                        frameLock.Release();
                    }
                };

                var encodingProfile = MediaEncodingProfile.CreateWmv(VideoEncodingQuality.HD1080p);
                encodingProfile.Video.Width = evenWidth;
                encodingProfile.Video.Height = evenHeight;

                var transcoder = new MediaTranscoder
                {
                    HardwareAccelerationEnabled = true,
                    AlwaysReencode = true
                };

                var prepareResult = await transcoder.PrepareMediaStreamSourceTranscodeAsync(
                    mediaStreamSource,
                    destinationStream.AsRandomAccessStream(),
                    encodingProfile);

                _screenCapturer.StartCapture(display, cancellationToken);
                await prepareResult.TranscodeAsync();
                
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex);
            }
        }
    }
}
