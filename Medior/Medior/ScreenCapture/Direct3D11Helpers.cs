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

using System.Runtime.InteropServices;
using Windows.Graphics.DirectX.Direct3D11;

namespace CaptureEncoder
{
    [ComImport]
    [Guid("A9B3D012-3DF2-4EE3-B8D1-8695F457D3C1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    interface IDirect3DDxgiInterfaceAccess
    {
        IntPtr GetInterface([In] ref Guid iid);
    };

    public static class Direct3D11Helpers
    {
        internal static Guid IInspectable = new("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90");
        internal static Guid ID3D11Resource = new("dc8e63f3-d12b-4952-b47b-5e45026a862d");
        internal static Guid IDXGIAdapter3 = new("645967A4-1392-4310-A798-8053CE3E93FD");
        internal static Guid ID3D11Device = new("db6f6ddb-ac77-4e88-8253-819df9bbf140");
        internal static Guid ID3D11Texture2D = new("6f15aaf2-d208-4e89-9ab4-489535d34f9c");

        [DllImport(
            "d3d11.dll",
            EntryPoint = "CreateDirect3D11DeviceFromDXGIDevice",
            SetLastError = true,
            CharSet = CharSet.Unicode,
            ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall
            )]
        internal static extern UInt32 CreateDirect3D11DeviceFromDXGIDevice(IntPtr dxgiDevice, out IntPtr graphicsDevice);

        [DllImport(
            "d3d11.dll",
            EntryPoint = "CreateDirect3D11SurfaceFromDXGISurface",
            SetLastError = true,
            CharSet = CharSet.Unicode,
            ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall
            )]
        internal static extern UInt32 CreateDirect3D11SurfaceFromDXGISurface(IntPtr dxgiSurface, out IntPtr graphicsSurface);

        public static IDirect3DDevice? CreateDevice()
        {
            return CreateDevice(false);
        }

        public static IDirect3DDevice? CreateDevice(bool useWARP)
        {
            var d3dDevice = new SharpDX.Direct3D11.Device(
                useWARP ? SharpDX.Direct3D.DriverType.Warp : SharpDX.Direct3D.DriverType.Hardware,
                SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport);
            IDirect3DDevice? device = null;

            // Acquire the DXGI interface for the Direct3D device.
            using (var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device3>())
            {
                // Wrap the native device using a WinRT interop object.
                uint hr = CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice.NativePointer, out IntPtr pUnknown);

                if (hr == 0)
                {
                    device = Marshal.GetObjectForIUnknown(pUnknown) as IDirect3DDevice;
                    Marshal.Release(pUnknown);
                }
            }

            return device;
        }

        internal static IDirect3DSurface? CreateDirect3DSurfaceFromSharpDXTexture(SharpDX.Direct3D11.Texture2D texture)
        {
            IDirect3DSurface? surface = null;

            // Acquire the DXGI interface for the Direct3D surface.
            using (var dxgiSurface = texture.QueryInterface<SharpDX.DXGI.Surface>())
            {
                // Wrap the native device using a WinRT interop object.
                uint hr = CreateDirect3D11SurfaceFromDXGISurface(dxgiSurface.NativePointer, out IntPtr pUnknown);

                if (hr == 0)
                {
                    surface = Marshal.GetObjectForIUnknown(pUnknown) as IDirect3DSurface;
                    Marshal.Release(pUnknown);
                }
            }

            return surface;
        }

        internal static SharpDX.Direct3D11.Device CreateSharpDXDevice(IDirect3DDevice device)
        {
            var access = (IDirect3DDxgiInterfaceAccess)device;
            var d3dPointer = access.GetInterface(ID3D11Device);
            var d3dDevice = new SharpDX.Direct3D11.Device(d3dPointer);
            return d3dDevice;
        }

        internal static SharpDX.Direct3D11.Texture2D CreateSharpDXTexture2D(IDirect3DSurface surface)
        {
            var access = (IDirect3DDxgiInterfaceAccess)surface;
            var d3dPointer = access.GetInterface(ID3D11Texture2D);
            var d3dSurface = new SharpDX.Direct3D11.Texture2D(d3dPointer);
            return d3dSurface;
        }
    }
}
