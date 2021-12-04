using System.Drawing;

namespace Medior.Models
{
    public class DisplayInfo
    {
        public int BitsPerPixel { get; set; }
        public Rectangle Bounds { get; set; }
        public string Name { get; set; } = String.Empty;
        public bool IsPrimary { get; set; }
    }
}
