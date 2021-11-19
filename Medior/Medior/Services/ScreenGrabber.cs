using Medior.BaseTypes;
using Medior.Models;
using Microsoft.Extensions.Logging;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Graphics.Capture;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Result = Medior.BaseTypes.Result;

namespace Medior.Services
{
    public enum ScreenGrabTarget
    {
        Display,
        Window,
        AllDisplays
    }

    public interface IScreenGrabber
    {
        Task<Result> EncodeVideo(GraphicsCaptureItem captureItem, string targetFilePath, CancellationToken cancellationToken);

        Result<Bitmap> GetScreenGrab(string targetName, ScreenGrabTarget targetType, bool useDirectX);
        IEnumerable<DisplayInfo> GetDisplays();
    }



    public class ScreenGrabber : IScreenGrabber
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<ScreenGrabber> _logger;

        public ScreenGrabber(IFileSystem fileSystem, ILogger<ScreenGrabber> logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public async Task<Result> EncodeVideo(
            GraphicsCaptureItem captureItem,
            string targetPath,
            CancellationToken cancellationToken)
        {
            try
            {
                _fileSystem.CreateDirectory(Path.GetDirectoryName(targetPath) ?? "");
                using var destStream = _fileSystem.CreateFile(targetPath);

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

                    var result = GetDirectXGrabFromDisplay(Screen.PrimaryScreen.DeviceName);

                    if (!result.IsSuccess || result.Value is null)
                    {
                        return;
                    }
                    
                    using var screenGrab = result.Value;
                    var bd = screenGrab.LockBits(new Rectangle(0, 0, bounds.Width, bounds.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

                    Marshal.Copy(bd.Scan0, tempArray, 0, size);

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


        public IEnumerable<DisplayInfo> GetDisplays()
        {
            return Screen.AllScreens.Select(x => new DisplayInfo()
            {
                Name = x.DeviceName,
                Bounds = x.Bounds,
                Primary = x.Primary,
                BitsPerPixel = x.BitsPerPixel
            });
        }

        public Result<Bitmap> GetScreenGrab(string targetName, ScreenGrabTarget targetType, bool useDirectX)
        {
            // TODO
            return Result.Ok<Bitmap>(null);
        }
        private void GetBitBltGrab(string displayName)
        {
            var bounds = new Rectangle();
            using var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using var graphics = Graphics.FromImage(bitmap);
            // TODO
        }

        private Result<Bitmap> GetDirectXGrabFromDisplay(string displayName)
        {
            try
            {
                using var factory = new Factory1();

                using var adapter = factory.Adapters1
                    .Where(x => (x.Outputs?.Length ?? 0) > 0)
                    .FirstOrDefault(x => x.Outputs.Any(o => o.Description.DeviceName == displayName));

                if (adapter is null)
                {
                    return Result.Fail<Bitmap>("Could not find the display.");
                }

                using var output = adapter.Outputs
                    .FirstOrDefault(x => x.Description.DeviceName == displayName);

                if (output is null)
                {
                    return Result.Fail<Bitmap>("Could not find the display.");
                }


                using var output1 = output.QueryInterface<Output1>();

                using var device = new SharpDX.Direct3D11.Device(adapter);
                
                var bounds = output1.Description.DesktopBounds;
                var width = bounds.Right - bounds.Left;
                var height = bounds.Bottom - bounds.Top;

                // Create Staging texture CPU-accessible
                var textureDesc = new Texture2DDescription
                {
                    CpuAccessFlags = CpuAccessFlags.Read,
                    BindFlags = BindFlags.None,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = width,
                    Height = height,
                    OptionFlags = ResourceOptionFlags.None,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Staging
                };

                var texture2D = new Texture2D(device, textureDesc);

                using var duplication = output1.DuplicateOutput(device);

                // Try to get duplicated frame within given time is ms
                var result = duplication.TryAcquireNextFrame(5000,
                    out var duplicateFrameInformation,
                    out var screenResource);

                if (result.Failure)
                {
                    if (result.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Code)
                    {
                        return Result.Fail<Bitmap>("Timeout.");
                    }
                    else
                    {
                        _logger.LogError("TryAcquireFrame error.  Code: {code}", result.Code);
                        return Result.Fail<Bitmap>($"Error code {result.Code}.");
                    }
                }

                if (duplicateFrameInformation.AccumulatedFrames == 0)
                {
                    try
                    {
                        duplication.ReleaseFrame();
                    }
                    catch { }
                    return Result.Fail<Bitmap>("No new frames arrived.");
                }

                var screenGrab = new Bitmap(texture2D.Description.Width, texture2D.Description.Height, PixelFormat.Format32bppArgb);

                using var screenTexture2D = screenResource.QueryInterface<Texture2D>();

                device.ImmediateContext.CopyResource(screenTexture2D, texture2D);

                var mapSource = device.ImmediateContext.MapSubresource(texture2D, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

                var boundsRect = new Rectangle(0, 0, texture2D.Description.Width, texture2D.Description.Height);

                var mapDest = screenGrab.LockBits(boundsRect, ImageLockMode.WriteOnly, screenGrab.PixelFormat);
                var sourcePtr = mapSource.DataPointer;
                var destPtr = mapDest.Scan0;
                for (int y = 0; y < texture2D.Description.Height; y++)
                {
                    SharpDX.Utilities.CopyMemory(destPtr, sourcePtr, texture2D.Description.Width * 4);

                    sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                    destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                }

                screenGrab.UnlockBits(mapDest);
                device.ImmediateContext.UnmapSubresource(texture2D, 0);

                screenResource.Dispose();
                duplication.ReleaseFrame();

                return Result.Ok(screenGrab);
            }
            catch (SharpDXException e)
            {
                if (e.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Code)
                {
                    return Result.Fail<Bitmap>("Timed out.");
                }
                _logger.LogError(e, "SharpDXException error.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while grabbing screen.");
            }
            return Result.Fail<Bitmap>("Failed to grab screen.");
        }
    }
}
