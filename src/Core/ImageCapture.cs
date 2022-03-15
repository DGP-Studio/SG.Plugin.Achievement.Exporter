using System;
using System.Drawing;

namespace Achievement.Exporter.Plugin
{
    public class ImageCapture
    {
        private IntPtr hwnd;
        private IntPtr hdc;

        public void Start()
        {
            hwnd = NativeMethod.GetDesktopWindow();
            hdc = NativeMethod.GetDC(hwnd);
        }

        public Bitmap Capture(int x, int y, int w, int h)
        {
            Bitmap bmp = new(w, h);
            Graphics bmpGraphic = Graphics.FromImage(bmp);
            IntPtr bmpHdc = bmpGraphic.GetHdc();

            //copy it
            _ = NativeMethod.StretchBlt(bmpHdc, 0, 0, w, h, hdc, x, y, w, h, NativeMethod.CopyPixelOperation.SourceCopy);
            bmpGraphic.ReleaseHdc();

            return bmp;
        }

        public void Stop()
        {
            NativeMethod.ReleaseDC(hwnd, hdc);
        }
    }
}
