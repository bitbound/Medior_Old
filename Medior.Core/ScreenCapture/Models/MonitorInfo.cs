using System.Numerics;
using Windows.Foundation;

namespace Medior.Core.ScreenCapture.Models
{
    public class MonitorInfo
    {
        public bool IsPrimary { get; set; }
        public Vector2 ScreenSize { get; set; }
        public Rect MonitorArea { get; set; }
        public Rect WorkArea { get; set; }
        public string? DeviceName { get; set; }
        public IntPtr Hmon { get; set; }
    }

}
