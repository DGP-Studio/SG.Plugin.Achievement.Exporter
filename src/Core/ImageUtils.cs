using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Achievement.Exporter.Plugin
{
    internal static class ImageUtils
    {
        public static Bitmap FromFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            MemoryStream stream = new(bytes);
            return (Bitmap)Image.FromStream(stream);
        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap, ImageFormat? imageFormat = null)
        {
            MemoryStream stream = new();
            bitmap.Save(stream, imageFormat ?? ImageFormat.Png);
            BitmapImage image = new();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }

        public static void Binary(this Bitmap bitmap)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color curColor = bitmap.GetPixel(i, j);
                    int ret = (int)(curColor.R * 0.299 + curColor.G * 0.587 + curColor.B * 0.114);

                    bitmap.SetPixel(i, j, Color.FromArgb(ret, ret, ret));
                }
            }
        }
    }
}
