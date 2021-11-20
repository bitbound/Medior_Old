using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Extensions
{
    public static class BitmapExtensions
    {
        public static Rectangle ToRectangle(this Bitmap self)
        {
            return new Rectangle(0, 0, self.Width, self.Height);
        }
    }
}
