using Microsoft.UI.Xaml;
using PInvoke;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using Windows.Services.Store;
using WinRT.Interop;

namespace Medior.Extensions
{
    public static class WindowExtensions
    {
        public static void CoreInitialize(this Window self, object target)
        {
            var hwnd = WindowNative.GetWindowHandle(self);
            InitializeWithWindow.Initialize(target, hwnd);
        }

        public static IntPtr GetWindowHandle(this Window self)
        {
            return WindowNative.GetWindowHandle(self);
        }

        public static void InitializeObject(this Window self, object target)
        {
            var hwnd = GetWindowHandle(self);
            InitializeWithWindow.Initialize(target, hwnd);
        }

        public static async Task<GraphicsCaptureItem> InvokeGraphicsCapturePicker(this Window self)
        {
            var hwnd = WindowNative.GetWindowHandle(self);
            var picker = new GraphicsCapturePicker();
            InitializeWithWindow.Initialize(picker, hwnd);
            return await picker.PickSingleItemAsync();
        }

        public static void Minimize(this Window self)
        {
            User32.ShowWindow(self.GetWindowHandle(), User32.WindowShowStyle.SW_MINIMIZE);
        }

        public static void Restore(this Window self)
        {
            User32.ShowWindow(self.GetWindowHandle(), User32.WindowShowStyle.SW_RESTORE);
        }

        public static void SetStoreContext(this Window self)
        {
            var hwnd = WindowNative.GetWindowHandle(self);
            var context = StoreContext.GetDefault();
            InitializeWithWindow.Initialize(context, hwnd);
        }
    }
}
