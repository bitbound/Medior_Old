using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using Windows.Services.Store;
using WinRT.Interop;

namespace Medior.Extensions
{
    public static class WindowExtensions
    {
        public static void SetWindowSize(this Window self, int width, int height)
        {
            var hwnd = WindowNative.GetWindowHandle(self);

            var dpi = PInvoke.User32.GetDpiForWindow(hwnd);
            float scalingFactor = (float)dpi / 96;
            width = (int)(width * scalingFactor);
            height = (int)(height * scalingFactor);

            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, height,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE);
        }

        public static void SetStoreContext(this Window self)
        {
            var hwnd = WindowNative.GetWindowHandle(self);
            var context = StoreContext.GetDefault();
            InitializeWithWindow.Initialize(context, hwnd);
        }

        public static async Task<GraphicsCaptureItem> InvokeGraphicsCapturePicker(this Window self)
        {
            var hwnd = WindowNative.GetWindowHandle(self);
            var picker = new GraphicsCapturePicker();
            InitializeWithWindow.Initialize(picker, hwnd);
            return await picker.PickSingleItemAsync();
        }
    }
}
