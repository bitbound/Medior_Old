using Medior.BaseTypes;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Forms;
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
                    MediaEncodingSubtypes.Argb32,
                    (uint)captureArea.Width,
                    (uint)captureArea.Height);

                var videoDescriptor = new VideoStreamDescriptor(sourceVideoProperties);
                var mediaStreamSource = new MediaStreamSource(videoDescriptor)
                {
                    BufferTime = TimeSpan.Zero
                };

                Bitmap? screenGrab = null;
                var grabLock = new SemaphoreSlim(1, 1);
                var grabSignal = new AutoResetEvent(false);

                capturer.OnFrameArrived += (sender, frame) =>
                {
                    try
                    {
                        grabLock.Wait();
                        screenGrab?.Dispose();
                        screenGrab = frame;
                        grabSignal.Set();
                    }
                    finally
                    {
                        grabLock.Release();
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
                            if (grabSignal.WaitOne(1000))
                            {
                                break;
                            }
                        }

                        grabLock.Wait();

                        if (screenGrab is null)
                        {
                            return;
                        }

                        var bd = screenGrab.LockBits(new Rectangle(Point.Empty, captureArea.Size), ImageLockMode.ReadOnly, screenGrab.PixelFormat);

                        Marshal.Copy(bd.Scan0, tempBuffer, 0, size);

                        screenGrab.UnlockBits(bd);

                        args.Request.Sample = MediaStreamSample.CreateFromBuffer(tempBuffer.AsBuffer(), stopwatch.Elapsed);
                    }
                    finally
                    {
                        grabLock.Release();
                    }
                };

                var mp4Profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
                mp4Profile.Video.FrameRate.Numerator = 15;


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
