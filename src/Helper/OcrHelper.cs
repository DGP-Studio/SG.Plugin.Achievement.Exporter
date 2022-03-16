using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Achievement.Exporter.Plugin.Helper
{
    internal class OcrHelper
    {
        public static async Task<OcrResult> RecognizeAsync(string path, OcrEngine engine)
        {
            StorageFile storageFile = await StorageFile.GetFileFromPathAsync(path);
            using (IRandomAccessStream randomAccessStream = await storageFile.OpenReadAsync())
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
                using (SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied))
                {
                    return await engine.RecognizeAsync(softwareBitmap);
                }
            }
        }
    }
}
