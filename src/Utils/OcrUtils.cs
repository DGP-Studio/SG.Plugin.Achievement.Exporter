using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Achievement.Exporter.Plugin
{
    internal class OcrUtils
    {
        public static async Task<OcrResult> RecognizeAsync(string path, OcrEngine engine)
        {
            StorageFile storageFile;
            storageFile = await StorageFile.GetFileFromPathAsync(path);
            IRandomAccessStream randomAccessStream = await storageFile.OpenReadAsync();
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
            SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            OcrResult ocrResult = await engine.RecognizeAsync(softwareBitmap);
            randomAccessStream.Dispose();
            softwareBitmap.Dispose();
            return ocrResult;
        }

        public static string LineString(OcrLine line)
        {
            string lineStr = "";
            foreach (var word in line.Words)
            {
                lineStr += word.Text;
            }
            return lineStr;
        }
    }
}
