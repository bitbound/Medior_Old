//MIT License

//Copyright (c) 2019 Robert Mikhayelyan

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using CommunityToolkit.Diagnostics;
using Windows.Graphics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;

namespace CaptureEncoder
{
    public sealed class CaptureFrameWait : IDisposable
    {
        private ManualResetEvent? _closedEvent;

        private SharpDX.Direct3D11.RenderTargetView? _composeRenderTargetView;

        private SharpDX.Direct3D11.Texture2D? _composeTexture;

        private Direct3D11CaptureFrame? _currentFrame;

        private SharpDX.Direct3D11.Device? _d3dDevice;

        private IDirect3DDevice? _device;

        private ManualResetEvent[] _events = Array.Empty<ManualResetEvent>();

        private ManualResetEvent? _frameEvent;

        private Direct3D11CaptureFramePool? _framePool;

        private GraphicsCaptureItem? _item;

        private SharpDX.Direct3D11.Multithread? _multithread;

        private GraphicsCaptureSession? _session;

        public CaptureFrameWait(
            IDirect3DDevice device,
            GraphicsCaptureItem item,
            SizeInt32 size,
            bool includeCursor)
        {
            _device = device;
            _d3dDevice = Direct3D11Helpers.CreateSharpDXDevice(device);
            _multithread = _d3dDevice.QueryInterface<SharpDX.Direct3D11.Multithread>();
            _multithread.SetMultithreadProtected(true);
            _item = item;
            _frameEvent = new ManualResetEvent(false);
            _closedEvent = new ManualResetEvent(false);
            _events = new[] { _closedEvent, _frameEvent };

            InitializeComposeTexture(size);
            InitializeCapture(size, includeCursor);
        }

        public void Dispose()
        {
            Stop();
            Cleanup();
        }

        public SurfaceWithInfo? WaitForNewFrame()
        {
            // Let's get a fresh one.
            _currentFrame?.Dispose();
            _frameEvent?.Reset();

            var signaledEvent = _events[WaitHandle.WaitAny(_events)];
            if (signaledEvent == _closedEvent)
            {
                Cleanup();
                return null;
            }

            Guard.IsNotNull(_currentFrame, nameof(_currentFrame));
            Guard.IsNotNull(_multithread, nameof(_multithread));
            Guard.IsNotNull(_d3dDevice, nameof(_d3dDevice));

            var result = new SurfaceWithInfo
            {
                SystemRelativeTime = _currentFrame.SystemRelativeTime
            };
            using (var multithreadLock = new MultithreadLock(_multithread))
            using (var sourceTexture = Direct3D11Helpers.CreateSharpDXTexture2D(_currentFrame.Surface))
            {
                _d3dDevice.ImmediateContext.ClearRenderTargetView(_composeRenderTargetView, new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 1));

                var width = Math.Clamp(_currentFrame.ContentSize.Width, 0, _currentFrame.Surface.Description.Width);
                var height = Math.Clamp(_currentFrame.ContentSize.Height, 0, _currentFrame.Surface.Description.Height);
                var region = new SharpDX.Direct3D11.ResourceRegion(0, 0, 0, width, height, 1);
                _d3dDevice.ImmediateContext.CopySubresourceRegion(sourceTexture, 0, region, _composeTexture, 0);

                var description = sourceTexture.Description;
                description.Usage = SharpDX.Direct3D11.ResourceUsage.Default;
                description.BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | SharpDX.Direct3D11.BindFlags.RenderTarget;
                description.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None;
                description.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;

                using var copyTexture = new SharpDX.Direct3D11.Texture2D(_d3dDevice, description);
                _d3dDevice.ImmediateContext.CopyResource(_composeTexture, copyTexture);
                result.Surface = Direct3D11Helpers.CreateDirect3DSurfaceFromSharpDXTexture(copyTexture);
            }

            return result;
        }

        private void Cleanup()
        {
            _framePool?.Dispose();
            _session?.Dispose();
            if (_item != null)
            {
                _item.Closed -= OnClosed;
            }
            _item = null;
            _device = null;
            _d3dDevice = null;
            _composeTexture?.Dispose();
            _composeTexture = null;
            _composeRenderTargetView?.Dispose();
            _composeRenderTargetView = null;
            _currentFrame?.Dispose();
        }

        private void InitializeCapture(SizeInt32 size, bool includeCursor)
        {
            Guard.IsNotNull(_item, nameof(_item));

            _item.Closed += OnClosed;
            _framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                _device,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                1,
                size);
            _framePool.FrameArrived += OnFrameArrived;
            _session = _framePool.CreateCaptureSession(_item);
            
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))
            {
                _session.IsCursorCaptureEnabled = includeCursor;
            }
            _session.StartCapture();
        }

        private void InitializeComposeTexture(SizeInt32 size)
        {
            var description = new SharpDX.Direct3D11.Texture2DDescription
            {
                Width = size.Width,
                Height = size.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                SampleDescription = new SharpDX.DXGI.SampleDescription()
                {
                    Count = 1,
                    Quality = 0
                },
                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | SharpDX.Direct3D11.BindFlags.RenderTarget,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None
            };
            _composeTexture = new SharpDX.Direct3D11.Texture2D(_d3dDevice, description);
            _composeRenderTargetView = new SharpDX.Direct3D11.RenderTargetView(_d3dDevice, _composeTexture);
        }

        private void OnClosed(GraphicsCaptureItem sender, object args)
        {
            Stop();
        }

        private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            SetResult(sender.TryGetNextFrame());
        }

        private void SetResult(Direct3D11CaptureFrame frame)
        {
            _currentFrame = frame;

            Guard.IsNotNull(_frameEvent, nameof(_frameEvent));
            _frameEvent.Set();
        }

        private void Stop()
        {
            Guard.IsNotNull(_closedEvent, nameof(_closedEvent));
            _closedEvent.Set();
        }
    }

    public sealed class SurfaceWithInfo : IDisposable
    {
        public IDirect3DSurface? Surface { get; internal set; }
        public TimeSpan SystemRelativeTime { get; internal set; }

        public void Dispose()
        {
            Surface?.Dispose();
            Surface = null;
        }
    }

    class MultithreadLock : IDisposable
    {
        private SharpDX.Direct3D11.Multithread? _multithread;

        public MultithreadLock(SharpDX.Direct3D11.Multithread multithread)
        {
            _multithread = multithread;
            _multithread?.Enter();
        }

        public void Dispose()
        {
            _multithread?.Leave();
            _multithread = null;
        }
    }
}
