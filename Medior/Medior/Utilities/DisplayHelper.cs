using Medior.Models;
using System.Windows.Forms;

namespace Medior.Utilities
{
    public static class DisplayHelper
    {
        public static IEnumerable<DisplayInfo> GetDisplays()
        {
            return Screen.AllScreens.Select(x => new DisplayInfo()
            {
                Name = x.DeviceName,
                Bounds = x.Bounds,
                IsPrimary = x.Primary,
                BitsPerPixel = x.BitsPerPixel
            });
        }
    }
}
