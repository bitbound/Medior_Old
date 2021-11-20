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

                var size = captureArea.Width * 4 * captureArea.Height;
                var tempBuffer = new byte[size];

                var transcoder = new MediaTranscoder
                {
                    HardwareAccelerationEnabled = true,
                    AlwaysReencode = true
                };

                var videoProperties = VideoEncodingProperties.CreateUncompressed(
                    MediaEncodingSubtypes.Argb32,
                    (uint)captureArea.Width,
                    (uint)captureArea.Height);

                var videoDescriptor = new VideoStreamDescriptor(videoProperties);
                var mediaStream = new MediaStreamSource(videoDescriptor);

                Bitmap? screenGrab = null;
                var grabLock = new SemaphoreSlim(1, 1);
                var grabSignal = new AutoResetEvent(false);
                
                ScreenCapturer.OnScreenUpdated += (sender, args) =>
                {
                    try
                    {
                        grabLock.Wait();
                        screenGrab?.Dispose();
                        screenGrab = (Bitmap)args.Bitmap.Clone();
                        grabSignal.Set();
                    }
                    finally
                    {
                        grabLock.Release();
                    }
                };

                mediaStream.SampleRequested += (sender, args) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        args.Request.Sample = null;
                        return;
                    }

                    try
                    {
                        grabSignal.WaitOne();
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
            var hwnd = IntPtr.Zero;
            var screenDc = new User32.SafeDCHandle();
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
