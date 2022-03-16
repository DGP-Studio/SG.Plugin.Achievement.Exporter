using System.Linq;
using Windows.Media.Ocr;

namespace Achievement.Exporter.Plugin.Helper
{
    internal static class OcrLineExtensions 
    {
        public static string Concat(this OcrLine line)
        {
            return string.Concat(line.Words.Select(word => word.Text));
        }
    }
}
