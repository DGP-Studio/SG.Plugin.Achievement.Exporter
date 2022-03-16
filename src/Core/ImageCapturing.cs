using Snap.Win32.NativeMethod;
using System;
using System.Drawing;

namespace Achievement.Exporter.Plugin
{
    /// <summary>
    /// 图像捕获
    /// </summary>
    public class ImageCapturing
    {
        private IntPtr hwnd;
        private IntPtr hdc;

        public void Start()
        {
            hwnd = User32.GetDesktopWindow();
            hdc = User32.GetDC(hwnd);
        }

        public Bitmap Capture(int left, int top, int width, int height)
        {
            Bitmap bmp = new(width, height);
            Graphics bmpGraphic = Graphics.FromImage(bmp);
            IntPtr bmpHdc = bmpGraphic.GetHdc();

            //copy it
            _ = GDI32.StretchBlt(bmpHdc, 0, 0, width, height, hdc, left, top, width, height, GDI32.CopyPixelOperation.SourceCopy);
            bmpGraphic.ReleaseHdc();

            return bmp;
        }

        public void Stop()
        {
            User32.ReleaseDC(hwnd, hdc);
        }
    }
}
