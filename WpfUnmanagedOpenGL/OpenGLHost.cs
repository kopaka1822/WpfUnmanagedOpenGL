using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace WpfUnmanagedOpenGL
{
    public class OpenGlHost : HwndHost
    {
        #region Custom Dll

        private const string DllFilePath = @"CppOpenGL.dll";

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern string get_error();

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool render();

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool initialize();

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool resize(int width, int height);

        #endregion

        #region Windows DLL

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
        internal static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateWindowEx(
            int dwExStyle,
            string lpszClassName,
            string lpszWindowName,
            int style,
            int x, int y,
            int width, int height,
            IntPtr hwndParent,
            IntPtr hMenu,
            IntPtr hInst,
            [MarshalAs(UnmanagedType.AsAny)] object pvParam
        );

        [DllImport("user32.dll", EntryPoint = "GetDC", CharSet = CharSet.Unicode)]
        internal static extern IntPtr GetDC(IntPtr hWnd);

        [Flags]
        public enum PFD_FLAGS : uint
        {
            PFD_DOUBLEBUFFER = 0x00000001,
            PFD_STEREO = 0x00000002,
            PFD_DRAW_TO_WINDOW = 0x00000004,
            PFD_DRAW_TO_BITMAP = 0x00000008,
            PFD_SUPPORT_GDI = 0x00000010,
            PFD_SUPPORT_OPENGL = 0x00000020,
            PFD_GENERIC_FORMAT = 0x00000040,
            PFD_NEED_PALETTE = 0x00000080,
            PFD_NEED_SYSTEM_PALETTE = 0x00000100,
            PFD_SWAP_EXCHANGE = 0x00000200,
            PFD_SWAP_COPY = 0x00000400,
            PFD_SWAP_LAYER_BUFFERS = 0x00000800,
            PFD_GENERIC_ACCELERATED = 0x00001000,
            PFD_SUPPORT_DIRECTDRAW = 0x00002000,
            PFD_DIRECT3D_ACCELERATED = 0x00004000,
            PFD_SUPPORT_COMPOSITION = 0x00008000,
            PFD_DEPTH_DONTCARE = 0x20000000,
            PFD_DOUBLEBUFFER_DONTCARE = 0x40000000,
            PFD_STEREO_DONTCARE = 0x80000000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PIXELFORMATDESCRIPTOR
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="colorBits">Specifies the number of color bitplanes in each color buffer. For RGBA pixel types, it is the size of the color buffer, excluding the alpha bitplanes. For color-index pixels, it is the size of the color-index buffer.</param>
            /// <param name="depthBits">Specifies the depth of the depth (z-axis) buffer.</param>
            /// <param name="stencilBits">Specifies the depth of the stencil buffer.</param>
            public void Init(byte colorBits, byte depthBits, byte stencilBits)
            {
                nSize = (ushort)Marshal.SizeOf(typeof(PIXELFORMATDESCRIPTOR));
                nVersion = 1;
                dwFlags = PFD_FLAGS.PFD_DRAW_TO_WINDOW | PFD_FLAGS.PFD_SUPPORT_OPENGL | PFD_FLAGS.PFD_DOUBLEBUFFER;
                iPixelType = PFD_PIXEL_TYPE.PFD_TYPE_RGBA;
                cColorBits = colorBits;
                cRedBits = cRedShift = cGreenBits = cGreenShift = cBlueBits = cBlueShift = 0;
                cAlphaBits = cAlphaShift = 0;
                cAccumBits = cAccumRedBits = cAccumGreenBits = cAccumBlueBits = cAccumAlphaBits = 0;
                cDepthBits = depthBits;
                cStencilBits = stencilBits;
                cAuxBuffers = 0;
                iLayerType = PFD_LAYER_TYPES.PFD_MAIN_PLANE;
                bReserved = 0;
                dwLayerMask = dwVisibleMask = dwDamageMask = 0;
            }
            ushort nSize;
            ushort nVersion;
            PFD_FLAGS dwFlags;
            PFD_PIXEL_TYPE iPixelType;
            byte cColorBits;
            byte cRedBits;
            byte cRedShift;
            byte cGreenBits;
            byte cGreenShift;
            byte cBlueBits;
            byte cBlueShift;
            byte cAlphaBits;
            byte cAlphaShift;
            byte cAccumBits;
            byte cAccumRedBits;
            byte cAccumGreenBits;
            byte cAccumBlueBits;
            byte cAccumAlphaBits;
            byte cDepthBits;
            byte cStencilBits;
            byte cAuxBuffers;
            PFD_LAYER_TYPES iLayerType;
            byte bReserved;
            uint dwLayerMask;
            uint dwVisibleMask;
            uint dwDamageMask;
        }

        public enum PFD_LAYER_TYPES : byte
        {
            PFD_MAIN_PLANE = 0,
            PFD_OVERLAY_PLANE = 1,
            PFD_UNDERLAY_PLANE = 255
        }

        public enum PFD_PIXEL_TYPE : byte
        {
            PFD_TYPE_RGBA = 0,
            PFD_TYPE_COLORINDEX = 1
        }


        internal const int
            WS_CHILD = 0x40000000,
            WS_VISIBLE = 0x10000000;

        [DllImport("opengl32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr wglCreateContext(IntPtr hDC);

        [DllImport("opengl32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool wglMakeCurrent(IntPtr hDC, IntPtr hRC);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int ChoosePixelFormat(IntPtr hDC, [In] ref PIXELFORMATDESCRIPTOR ppfd);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool SetPixelFormat(IntPtr hDC, int iPixelFormat, ref PIXELFORMATDESCRIPTOR ppfd);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool SwapBuffers(IntPtr hDC);

        #endregion

        // host of the HwndHost
        private readonly Border parent;

        // context creation
        private IntPtr hWnd = IntPtr.Zero;
        private IntPtr deviceContext = IntPtr.Zero;

        // render thread (aynchronous openGL drawing)
        private Thread renderThread;
        private bool isRunning = true;

        // helper to detect resize in render thread
        int renderWidth = 0;
        int renderHeight = 0;

        public OpenGlHost(Border parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// asynchronous thread for openGL rendering
        /// </summary>
        /// <param name="data">null (required for c# thread)</param>
        private void Render(object data)
        {
            try
            {
                InitializeOpenGl();

                while (isRunning)
                {
                    HandleResize();

                    // dll render call
                    if (!render())
                        throw new Exception(get_error());

                    if(!SwapBuffers(deviceContext))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// openGL initialization (pixel format and wgl context)
        /// </summary>
        private void InitializeOpenGl()
        {
            deviceContext = GetDC(hWnd);

            PIXELFORMATDESCRIPTOR pfd = new PIXELFORMATDESCRIPTOR();
            pfd.Init(
                24, // color bits
                24, // depth bits
                8 // stencil bits
            );

            var pixelFormat = ChoosePixelFormat(deviceContext, ref pfd);
            if (!SetPixelFormat(deviceContext, pixelFormat, ref pfd))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            var renderingContext = wglCreateContext(deviceContext);
            if(renderingContext == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            if(!wglMakeCurrent(deviceContext, renderingContext))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            // dll call: initialize glad etc.
            if(!initialize())
                throw new Exception(get_error());
        }

        /// <summary>
        /// calls the dll resize() function if the client area was resized
        /// </summary>
        private void HandleResize()
        {
            // viewport resize?
            int newWidth = (int)ActualWidth;
            int newHeight = (int)ActualHeight;

            if (renderWidth != newWidth || renderHeight != newHeight)
            {
                renderWidth = newWidth;
                renderHeight = newHeight;
                if (!resize(renderWidth, renderHeight))
                    throw new Exception(get_error());
            }
        }

        /// <summary>
        /// construction of the child window
        /// </summary>
        /// <param name="hwndParent">handle of parent window (border)</param>
        /// <returns></returns>
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            hWnd = CreateWindowEx(
                0, // dwstyle
                "static", // class name
                "", // window name
                WS_CHILD | WS_VISIBLE, // style
                0, // x
                0, // y
                (int) parent.ActualWidth, // renderWidth
                (int) parent.ActualHeight, // renderHeight
                hwndParent.Handle, // parent handle
                IntPtr.Zero, // menu
                IntPtr.Zero, // hInstance
                0 // param
            ); 

            renderThread = new Thread(new ParameterizedThreadStart(Render));
            renderThread.Start();

            return new HandleRef(this, hWnd);
        }

        /// <summary>
        /// stops render thread and performs destruction of window
        /// </summary>
        /// <param name="hwnd"></param>
        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            // stop render thread
            isRunning = false;
            renderThread.Join();

            // destroy resources
            DestroyWindow(hwnd.Handle);
        }
    }
}
