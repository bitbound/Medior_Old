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
using Windows.UI.Composition;

namespace CaptureEncoder
{
    public sealed class CapturePreview : IDisposable
    {
        private SharpDX.Direct3D11.Device? _d3dDevice;

        private IDirect3DDevice? _device;

        private Direct3D11CaptureFramePool? _framePool;

        private bool _includeCursor = true;

        private GraphicsCaptureItem? _item;

        private SizeInt32 _lastSize;

        private GraphicsCaptureSession? _session;

        private SharpDX.DXGI.SwapChain1? _swapChain;

        public CapturePreview(IDirect3DDevice device, GraphicsCaptureItem item)
        {
            _item = item;
            _device = device;
            _d3dDevice = Direct3D11Helpers.CreateSharpDXDevice(device);

            var dxgiDevice = _d3dDevice.QueryInterface<SharpDX.DXGI.Device>();
            var adapter = dxgiDevice.GetParent<SharpDX.DXGI.Adapter>();
            var factory = adapter.GetParent<SharpDX.DXGI.Factory2>();

            var description = new SharpDX.DXGI.SwapChainDescription1
            {
                Width = item.Size.Width,
                Height = item.Size.Height,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                SampleDescription = new SharpDX.DXGI.SampleDescription()
                {
                    Count = 1,
                    Quality = 0
                },
                BufferCount = 2,
                Scaling = SharpDX.DXGI.Scaling.Stretch,
                SwapEffect = SharpDX.DXGI.SwapEffect.FlipSequential,
                AlphaMode = SharpDX.DXGI.AlphaMode.Premultiplied
            };
            _swapChain = new SharpDX.DXGI.SwapChain1(factory, dxgiDevice, ref description);

            _framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                    _device,
                    DirectXPixelFormat.B8G8R8A8UIntNormalized,
                    2,
                    item.Size);
            _session = _framePool.CreateCaptureSession(item);
            _lastSize = item.Size;

            _framePool.FrameArrived += OnFrameArrived;
        }

        public bool IsCursorCaptureEnabled
        {
            get { return _includeCursor; }
            set
            {
                if (_includeCursor != value)
                {
                    _includeCursor = value;
                    if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))
                    {
                        Guard.IsNotNull(_session, nameof(_session));
                        _session.IsCursorCaptureEnabled = _includeCursor;
                    }
                }
            }
        }

        public GraphicsCaptureItem? Target => _item;
        public ICompositionSurface CreateSurface(Compositor compositor)
        {
            Guard.IsNotNull(_swapChain, nameof(_swapChain));
            return compositor.CreateCompositionSurfaceForSwapChain(_swapChain);
        }

        public void Dispose()
        {
            _session?.Dispose();
            _framePool?.Dispose();
            _swapChain?.Dispose();

            _swapChain = null;
            _framePool = null;
            _session = null;
            _item = null;
        }

        public void StartCapture()
        {
            Guard.IsNotNull(_session, nameof(_session));
            _session.StartCapture();
        }
        private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            Guard.IsNotNull(_swapChain, nameof(_swapChain));
            Guard.IsNotNull(_d3dDevice, nameof(_d3dDevice));
            Guard.IsNotNull(_framePool, nameof(_framePool));

            var newSize = false;

            using (var frame = sender.TryGetNextFrame())
            {
                if (frame.ContentSize.Width != _lastSize.Width ||
                    frame.ContentSize.Height != _lastSize.Height)
                {
                    // The thing we have been capturing has changed size.
                    // We need to resize our swap chain first, then blit the pixels.
                    // After we do that, retire the frame and then recreate our frame pool.
                    newSize = true;
                    _lastSize = frame.ContentSize;

                    _swapChain.ResizeBuffers(
                        2,
                        _lastSize.Width,
                        _lastSize.Height,
                        SharpDX.DXGI.Format.B8G8R8A8_UNorm, 
                        SharpDX.DXGI.SwapChainFlags.None);
                }


                using var sourceTexture = Direct3D11Helpers.CreateSharpDXTexture2D(frame.Surface);
                using var backBuffer = _swapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0);
                using var renderTargetView = new SharpDX.Direct3D11.RenderTargetView(_d3dDevice, backBuffer);
                _d3dDevice.ImmediateContext.ClearRenderTargetView(renderTargetView, new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 1));
                _d3dDevice.ImmediateContext.CopyResource(sourceTexture, backBuffer);

            } // retire the frame

            _swapChain.Present(1, SharpDX.DXGI.PresentFlags.None);

            if (newSize)
            {
                _framePool.Recreate(
                    _device,
                    DirectXPixelFormat.B8G8R8A8UIntNormalized,
                    2,
                    _lastSize);
            }
        }
    }
}
