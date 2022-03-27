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

        /// <summary>
        /// 获取一次捕获会话，在会话期间可以正常采样
        /// </summary>
        /// <returns></returns>
        public IDisposable Session()
        {
            this.hwnd = User32.GetDesktopWindow();
            this.hdc = User32.GetDC(this.hwnd);
            return new ImageCapturingDisposable(this);
        }

        /// <summary>
        /// 采样
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Bitmap Sample(int left, int top, int width, int height)
        {
            Bitmap bmp = new(width, height);
            using (Graphics bmpGraphic = Graphics.FromImage(bmp))
            {
                IntPtr bmpHdc = bmpGraphic.GetHdc();
                _ = GDI32.StretchBlt(bmpHdc, 0, 0, width, height, this.hdc, left, top, width, height, GDI32.CopyPixelOperation.SourceCopy);
            }

            return bmp;
        }

        private void Stop()
        {
            User32.ReleaseDC(this.hwnd, this.hdc);
        }

        private class ImageCapturingDisposable : IDisposable
        {
            private readonly ImageCapturing _imageCapturing;
            public ImageCapturingDisposable(ImageCapturing imageCapturing)
            {
                this._imageCapturing = imageCapturing;
            }
            public void Dispose()
            {
                this._imageCapturing.Stop();
            }
        }
    }
}
