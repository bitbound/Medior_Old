using Medior.BaseTypes;
using Medior.Extensions;
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
            int displayIndex,
            int adapterIndex, 
            Stream destinationStream, 
            CancellationToken cancellationToken);
    }

    public class ScreenRecorder : IScreenRecorder
    {
        public async Task<Result> CaptureVideo(
            int displayIndex,
            int adapterIndex,
            Stream destinationStream,
            CancellationToken cancellationToken)
        {
            try
            {
                var capturer = new ScreenCapturer();

                var captureArea = Screen.PrimaryScreen.Bounds;
                var size = captureArea.Width * 4 * captureArea.Height;
                var tempBuffer = new byte[size];

                var sourceVideoProperties = VideoEncodingProperties.CreateUncompressed(
                    MediaEncodingSubtypes.Jpeg,
                    (uint)captureArea.Width,
                    (uint)captureArea.Height);

                var videoDescriptor = new VideoStreamDescriptor(sourceVideoProperties);
                
                var mediaStreamSource = new MediaStreamSource(videoDescriptor)
                {
                    BufferTime = TimeSpan.Zero,
                };


                Bitmap? currentFrame = null;
                var frameLock = new SemaphoreSlim(1, 1);
                var frameSignal = new AutoResetEvent(false);

                capturer.OnFrameArrived += (sender, frame) =>
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

                capturer.StartCapture(0, 0, cancellationToken);
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
