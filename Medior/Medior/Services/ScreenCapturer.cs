using System.Drawing;
using System.Drawing.Imaging;
using Medior.Models;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Resource = SharpDX.DXGI.Resource;

namespace Medior.Services
{
    public interface IScreenCapturer
    {
        bool IsCapturing { get; }

        event EventHandler<Exception>? OnException;
        event EventHandler<Bitmap>? OnFrameArrived;

        void StartCapture(DisplayInfo display, CancellationToken cancellationToken = default);
        void StopCapture();
    }

    public class ScreenCapturer : IScreenCapturer
    {

        private Thread? _captureThread;

        public event EventHandler<Bitmap>? OnFrameArrived;
        public event EventHandler<Exception>? OnException;

        public bool IsCapturing { get; private set; }

        public void StartCapture(
            DisplayInfo display,
            CancellationToken cancellationToken = default)
        {

            if (IsCapturing)
            {
                return;
            }

            IsCapturing = true;

            _captureThread = new Thread(() => CaptureInternal(display, cancellationToken))
            {
                Priority = ThreadPriority.Highest
            };
            _captureThread.Start();
        }

        public void StopCapture()
        {
            IsCapturing = false;
        }

        private void CaptureInternal(DisplayInfo display, CancellationToken cancellationToken)
        {
            Resource? screenResource = null;
            try
            {
                int adapterIndex = -1;
                int outputIndex = -1;
                using var factory1 = new Factory1();

                for (var ai = 0; ai < factory1.Adapters1.Length; ai++)
                {
                    for (var oi = 0; oi < factory1.Adapters1[ai].Outputs.Length; oi++)
                    {
                        if (factory1.Adapters1[ai].Outputs[oi].Description.DeviceName == display.Name)
                        {
                            adapterIndex = ai;
                            outputIndex = oi;
                        }
                    }
                }

                if (outputIndex == -1)
                {
                    return;
                }

                using var adapter1 = factory1.GetAdapter1(adapterIndex);
                using var device = new Device(adapter1);
                using var output = adapter1.GetOutput(outputIndex);
                using var output1 = output.QueryInterface<Output1>();
                var width = output1.Description.DesktopBounds.Right - output1.Description.DesktopBounds.Left;
                var height = output1.Description.DesktopBounds.Bottom - output1.Description.DesktopBounds.Top;
                var bounds = new Rectangle(Point.Empty, new Size(width, height));
                var texture2DDescription = new Texture2DDescription
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

                using var texture2D = new Texture2D(device, texture2DDescription);
                using var outputDuplication = output1.DuplicateOutput(device);

                while (IsCapturing && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = outputDuplication.TryAcquireNextFrame(100, out _, out screenResource);

                        if (!result.Success)
                        {
                            continue;
                        }

                        using Texture2D screenTexture2D = screenResource.QueryInterface<Texture2D>();
                        device.ImmediateContext.CopyResource(screenTexture2D, texture2D);
                        var dataBox = device.ImmediateContext.MapSubresource(texture2D, 0, MapMode.Read, MapFlags.None);
                        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);
                        var bitmapData = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                        var dataBoxPointer = dataBox.DataPointer;
                        var bitmapDataPointer = bitmapData.Scan0;
                        for (var y = 0; y < height; y++)
                        {
                            SharpDX.Utilities.CopyMemory(bitmapDataPointer, dataBoxPointer, width * 4);
                            dataBoxPointer = IntPtr.Add(dataBoxPointer, dataBox.RowPitch);
                            bitmapDataPointer = IntPtr.Add(bitmapDataPointer, bitmapData.Stride);
                        }
                        bitmap.UnlockBits(bitmapData);
                        device.ImmediateContext.UnmapSubresource(texture2D, 0);

                        OnFrameArrived?.Invoke(this, bitmap);
                    }
                    catch (Exception ex)
                    {
                        OnException?.Invoke(this, ex);
                    }
                    finally
                    {
                        screenResource?.Dispose();
                        try
                        {
                            outputDuplication.ReleaseFrame();
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, ex);
            }
            finally
            {
                IsCapturing = false;
                _captureThread = null;
            }
        }
    }
}
