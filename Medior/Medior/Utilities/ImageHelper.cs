using Medior.BaseTypes;
using Medior.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Utilities
{
    public class ImageHelper
    {
        public static Result<Bitmap> GetImageDiff(Bitmap currentFrame, Bitmap? previousFrame)
        {
            if (currentFrame is null)
            {
                return Result.Fail<Bitmap>("Current frame cannot be empty.");
            }

            if (previousFrame is null)
            {
                return Result.Ok((Bitmap)currentFrame.Clone());
            }

            if (currentFrame.Height != previousFrame.Height || 
                currentFrame.Width != previousFrame.Width ||
                currentFrame.PixelFormat != previousFrame.PixelFormat)
            {
                return Result.Fail<Bitmap>("Frames must be of equal dimensions and format.");
            }

            var bytesPerPixel = Image.GetPixelFormatSize(currentFrame.PixelFormat) / 8;

            if (bytesPerPixel != 4)
            {
                return Result.Fail<Bitmap>("Images must be 4 bytes per pixel.");
            }

            var width = currentFrame.Width;
            var height = currentFrame.Height;

            var mergedFrame = new Bitmap(width, height);

            var bd1 = previousFrame.LockBits(previousFrame.ToRectangle(), ImageLockMode.ReadOnly, previousFrame.PixelFormat);
            var bd2 = currentFrame.LockBits(currentFrame.ToRectangle(), ImageLockMode.ReadOnly, currentFrame.PixelFormat);
            var bd3 = mergedFrame.LockBits(mergedFrame.ToRectangle(), ImageLockMode.WriteOnly, mergedFrame.PixelFormat);

            var totalSize = bd1.Height * bd1.Width * bytesPerPixel;

            try
            {
                unsafe
                {
                    byte* scan1 = (byte*)bd1.Scan0.ToPointer();
                    byte* scan2 = (byte*)bd2.Scan0.ToPointer();
                    byte* scan3 = (byte*)bd3.Scan0.ToPointer();

                    for (int counter = 0; counter < totalSize - bytesPerPixel; counter += bytesPerPixel)
                    {
                        byte* data1 = scan1 + counter;
                        byte* data2 = scan2 + counter;
                        byte* data3 = scan3 + counter;

                        if (data1[0] != data2[0] ||
                            data1[1] != data2[1] ||
                            data1[2] != data2[2] ||
                            data1[3] != data2[3])
                        {
                            data3[0] = data2[0];
                            data3[1] = data2[1];
                            data3[2] = data2[2];
                            data3[3] = data2[3];
                        }
                    }
                }

                return Result.Ok(mergedFrame);

            }
            catch
            {
                return Result.Fail<Bitmap>("Error while getting diff.");
            }
            finally
            {
                previousFrame.UnlockBits(bd1);
                currentFrame.UnlockBits(bd2);
                mergedFrame.UnlockBits(bd3);
            }
        }
        public static Result<bool> HasDifferences(Bitmap? currentFrame, Bitmap? previousFrame)
        {
            if (currentFrame is null || previousFrame is null)
            {
                return Result.Fail<bool>("Neither frame can be empty.");
            }

            if (currentFrame.Height != previousFrame.Height ||
                currentFrame.Width != previousFrame.Width ||
                currentFrame.PixelFormat != previousFrame.PixelFormat)
            {
                return Result.Fail<bool>("Frames must be of equal dimensions and format.");
            }

            var bytesPerPixel = Image.GetPixelFormatSize(currentFrame.PixelFormat) / 8;

            if (bytesPerPixel != 4)
            {
                return Result.Fail<bool>("Images must be 4 bytes per pixel.");
            }

            var width = currentFrame.Width;
            var height = currentFrame.Height;

            var bd1 = previousFrame.LockBits(previousFrame.ToRectangle(), ImageLockMode.ReadOnly, previousFrame.PixelFormat);
            var bd2 = currentFrame.LockBits(currentFrame.ToRectangle(), ImageLockMode.ReadOnly, currentFrame.PixelFormat);

            var totalSize = bd1.Height * bd1.Width * bytesPerPixel;

            try
            {
                unsafe
                {
                    byte* scan1 = (byte*)bd1.Scan0.ToPointer();
                    byte* scan2 = (byte*)bd2.Scan0.ToPointer();

                    for (int counter = 0; counter < totalSize - bytesPerPixel; counter++)
                    {
                        byte* data1 = scan1 + counter;
                        byte* data2 = scan2 + counter;

                        if (data1[0] != data2[0])
                        {
                            return Result.Ok(true);
                        }
                    }
                }

                return Result.Ok(false);

            }
            catch
            {
                return Result.Fail<bool>("Error while getting diff.");
            }
            finally
            {
                previousFrame.UnlockBits(bd1);
                currentFrame.UnlockBits(bd2);
            }
        }
    }
}
