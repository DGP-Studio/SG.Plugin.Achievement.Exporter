using System;
using System.Runtime.InteropServices;

namespace Achievement.Exporter.Plugin
{
    internal class NativeMethod
    {
        [DllImport("GDI32.DLL", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool StretchBlt(IntPtr hdcDest, int nXDest, int nYDest, int nDestWidth, int nDestHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int nSrcWidth, int nSrcHeight, CopyPixelOperation dwRop);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern IntPtr BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern IntPtr GetDesktopWindow();

        [Flags]
        internal enum CopyPixelOperation
        {
            NoMirrorBitmap = -2147483648,
            Blackness = 66,
            NotSourceErase = 1114278,
            NotSourceCopy = 3342344,
            SourceErase = 4457256,
            DestinationInvert = 5570569,
            PatInvert = 5898313,
            SourceInvert = 6684742,
            SourceAnd = 8913094,
            MergePaint = 12255782,
            MergeCopy = 12583114,
            SourceCopy = 13369376,
            SourcePaint = 15597702,
            PatCopy = 15728673,
            PatPaint = 16452105,
            Whiteness = 16711778,
            CaptureBlt = 1073741824,
        }

        [DllImport("user32", EntryPoint = "mouse_event")]
        public static extern int MouseEvent(MouseEventFlag dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll ")]
        public static extern IntPtr FindWindowEx(IntPtr parent, IntPtr childe, string strclass, string FrmText);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        internal static extern bool HideCaret(IntPtr controlHandle);

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow( IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);
    }
}
