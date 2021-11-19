using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models
{
    public class DisplayInfo
    {
        public int BitsPerPixel { get; set; }
        public Rectangle Bounds { get; set; }
        public string Name { get; set; } = String.Empty;
        public bool Primary { get; set; }
    }
}
