using System.Drawing;

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
