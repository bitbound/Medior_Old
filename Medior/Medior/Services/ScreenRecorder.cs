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
            Stream destinationStream, 
            CancellationToken cancellationToken)
        {
            try
            {

                var captureArea = new Rectangle(Point.Empty, display.Bounds.Size);

                var size = captureArea.Width * 4 * captureArea.Height;
                var tempBuffer = new byte[size];

                var sourceVideoProperties = VideoEncodingProperties.CreateUncompressed(
                    MediaEncodingSubtypes.Argb32,
                    (uint)captureArea.Width,
                    (uint)captureArea.Height);

                var videoDescriptor = new VideoStreamDescriptor(sourceVideoProperties);
                
                var mediaStreamSource = new MediaStreamSource(videoDescriptor)
                {
                    BufferTime = TimeSpan.Zero
                };


                Bitmap? currentFrame = null;
                var frameLock = new SemaphoreSlim(1, 1);
                var frameSignal = new AutoResetEvent(false);

                _screenCapturer.OnFrameArrived += (sender, frame) =>
                {
                    try
                    {
                        frameLock.Wait();
                        currentFrame?.Dispose();
                        frame.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        currentFrame = frame;
                        frameSignal.Set();
                    }
                    finally
                    {
                        frameLock.Release();
                    }
                };

                var stopwatch = Stopwatch.StartNew();
                var frames = 0;

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
                        frames++;
                    }
                    finally
                    {
                        frameLock.Release();
                    }
                };

                var mp4Profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);

                var transcoder = new MediaTranscoder
                {
                    HardwareAccelerationEnabled = true,
                    AlwaysReencode = true
                };

                var prepareResult = await transcoder.PrepareMediaStreamSourceTranscodeAsync(
                    mediaStreamSource,
                    destinationStream.AsRandomAccessStream(),
                    mp4Profile);

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
