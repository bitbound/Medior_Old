using Medior.BaseTypes;
using Medior.Models;
using Microsoft.Extensions.Logging;
using PInvoke;
using System.Drawing;
using System.Windows.Forms;
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
        Result<Bitmap> GetWinFormsGrab(Rectangle captureArea);
        Result<Bitmap> GetBitBltGrab(Rectangle captureArea);
        IEnumerable<DisplayInfo> GetDisplays();

    }

    public class ScreenGrabber : IScreenGrabber
    {
        private readonly ILogger<ScreenGrabber> _logger;

        public ScreenGrabber(ILogger<ScreenGrabber> logger)
        {
            _logger = logger;
        }

        public IEnumerable<DisplayInfo> GetDisplays()
        {
            return Screen.AllScreens.Select(x => new DisplayInfo()
            {
                Name = x.DeviceName,
                Bounds = x.Bounds,
                IsPrimary = x.Primary,
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
                _ = User32.ReleaseDC(hwnd, screenDc.HWnd);
            }
        }
    }
}
