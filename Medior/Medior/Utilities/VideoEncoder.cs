using Medior.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using System.Diagnostics;
using Windows.Storage.Streams;

namespace Medior.Utilities
{
    public static class VideoEncoder
    {
        public static async Task<Result> Encode(
            GraphicsCaptureItem captureItem, 
            string targetPath,
            CancellationToken cancellationToken)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? "");
                using var destStream = File.Create(targetPath);

                var bounds = captureItem.Size;
                var stopwatch = Stopwatch.StartNew();

                using var bitmap = new Bitmap(bounds.Width, bounds.Height);
                using var graphics = Graphics.FromImage(bitmap);
                var size = bounds.Width * Image.GetPixelFormatSize(bitmap.PixelFormat) / 8 * bounds.Height;
                var tempArray = new byte[size];

                var transcoder = new MediaTranscoder
                {
                    HardwareAccelerationEnabled = true
                };

                var videoProperties = VideoEncodingProperties.CreateUncompressed(
                    MediaEncodingSubtypes.Argb32,
                    (uint)bounds.Width,
                    (uint)bounds.Height);

                var videoDescriptor = new VideoStreamDescriptor(videoProperties);
                var mediaStream = new MediaStreamSource(videoDescriptor);

                mediaStream.SampleRequested += (sender, args) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        args.Request.Sample = null;
                        return;
                    }

                    graphics.CopyFromScreen(Point.Empty, Point.Empty, new Size(bounds.Width, bounds.Height));

                    var bd = bitmap.LockBits(new Rectangle(0, 0, bounds.Width, bounds.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                    Marshal.Copy(bd.Scan0, tempArray, 0, size);
                    bitmap.UnlockBits(bd);

                    args.Request.Sample = MediaStreamSample.CreateFromBuffer(tempArray.AsBuffer(), stopwatch.Elapsed);
                };

                var mp4Profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);

                var prepareResult = await transcoder.PrepareMediaStreamSourceTranscodeAsync(
                    mediaStream,
                    destStream.AsRandomAccessStream(), 
                    mp4Profile);

                await prepareResult.TranscodeAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex);
            }
        }
    }
}
