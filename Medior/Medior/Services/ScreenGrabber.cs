using Medior.BaseTypes;
using Medior.Models;
using Microsoft.Extensions.Logging;
using PInvoke;
using ScreenCapturerNS;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Graphics.Capture;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Result = Medior.BaseTypes.Result;

namespace Medior.Services
{
    public enum ScreenGrabTarget
    {
        Display,
        Window,
        AllDisplays
    }

    public interface IScreenGrabber
    {
        Task<Result> CaptureVideo(Rectangle captureArea, string targetFilePath, CancellationToken cancellationToken);

        Result<Bitmap> GetWinFormsGrab(Rectangle captureArea);
        Result<Bitmap> GetBitBltGrab(Rectangle captureArea);

        IEnumerable<DisplayInfo> GetDisplays();

    }



    public class ScreenGrabber : IScreenGrabber
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<ScreenGrabber> _logger;

        public ScreenGrabber(IFileSystem fileSystem, ILogger<ScreenGrabber> logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public async Task<Result> CaptureVideo(
            Rectangle captureArea,
            string targetPath,
            CancellationToken cancellationToken)
        {
            try
            {
                _fileSystem.CreateDirectory(Path.GetDirectoryName(targetPath) ?? "");
                using var destStream = _fileSystem.CreateFile(targetPath);

                var stopwatch = Stopwatch.StartNew();


                using var bitmap = new Bitmap(captureArea.Width, captureArea.Height);
                using var graphics = Graphics.FromImage(bitmap);
                var size = captureArea.Width * Image.GetPixelFormatSize(bitmap.PixelFormat) / 8 * captureArea.Height;
                var tempArray = new byte[size];

                var transcoder = new MediaTranscoder
                {
                    HardwareAccelerationEnabled = true
                };

                var videoProperties = VideoEncodingProperties.CreateUncompressed(
                    MediaEncodingSubtypes.Argb32,
                    (uint)captureArea.Width,
                    (uint)captureArea.Height);

                var videoDescriptor = new VideoStreamDescriptor(videoProperties);
                var mediaStream = new MediaStreamSource(videoDescriptor);

                var screenGrab = new Bitmap(captureArea.Width, captureArea.Height);
                var grabs = new ConcurrentQueue<Bitmap>();
                
                ScreenCapturer.OnScreenUpdated += (sender, args) =>
                {
                    while (grabs.Count > 1)
                    {
                        grabs.TryDequeue(out var staleBitmap);
                        staleBitmap?.Dispose();
                    }
                    grabs.Enqueue(args.Bitmap);
                };

                mediaStream.SampleRequested += (sender, args) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        args.Request.Sample = null;
                        return;
                    }

                    // TODO: Make copy.
                    Bitmap? latestGrab;

                    while (!grabs.TryDequeue(out latestGrab))
                    {
                        Thread.Sleep(1);
                    }


                    var bd = latestGrab.LockBits(new Rectangle(Point.Empty, captureArea.Size), ImageLockMode.ReadOnly, latestGrab.PixelFormat);

                    Marshal.Copy(bd.Scan0, tempArray, 0, size);

                    latestGrab.UnlockBits(bd);

                    args.Request.Sample = MediaStreamSample.CreateFromBuffer(tempArray.AsBuffer(), stopwatch.Elapsed);
                };

                var mp4Profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
                
                var prepareResult = await transcoder.PrepareMediaStreamSourceTranscodeAsync(
                    mediaStream,
                    destStream.AsRandomAccessStream(),
                    mp4Profile);

                ScreenCapturer.StartCapture();
                await prepareResult.TranscodeAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex);
            }
            finally
            {
                ScreenCapturer.StopCapture();
            }
        }


        public IEnumerable<DisplayInfo> GetDisplays()
        {
            return Screen.AllScreens.Select(x => new DisplayInfo()
            {
                Name = x.DeviceName,
                Bounds = x.Bounds,
                Primary = x.Primary,
                BitsPerPixel = x.BitsPerPixel
            });
        }

        public Result<Bitmap> GetWinFormsGrab(Rectangle captureArea)
        {
            try
            {
                var bitmap = new Bitmap(captureArea.Width, captureArea.Height);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(captureArea.Location, Point.Empty, captureArea.Size);
                return Result.Ok(bitmap);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error grabbing with BitBlt.");
                return Result.Fail<Bitmap>("Error grabbing with BitBlt.");
            }
        }


        public Result<Bitmap> GetBitBltGrab(Rectangle captureArea)
        {
            IntPtr hwnd = IntPtr.Zero;
            User32.SafeDCHandle screenDc = new User32.SafeDCHandle();
            try
            {
                hwnd = User32.GetDesktopWindow();
                screenDc = User32.GetWindowDC(hwnd);
                var bitmap = new Bitmap(captureArea.Width, captureArea.Height);
                using var graphics = Graphics.FromImage(bitmap);
                var targetDc = graphics.GetHdc();
                Gdi32.BitBlt(targetDc, 0, 0, captureArea.Width, captureArea.Height,
                    screenDc.DangerousGetHandle(), 0, 0, unchecked((int)CopyPixelOperation.SourceCopy));

                graphics.ReleaseHdc(targetDc);

                return Result.Ok(bitmap);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error grabbing with BitBlt.");
                return Result.Fail<Bitmap>("Error grabbing with BitBlt.");
            }
            finally
            {
                User32.ReleaseDC(hwnd, screenDc.HWnd);
            }
        }
    }
}
