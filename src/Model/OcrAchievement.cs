using Achievement.Exporter.Plugin.Helper;
using Snap.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Ocr;

namespace Achievement.Exporter.Plugin.Model
{
    [Serializable]
    public class OcrAchievement : ICloneable<OcrAchievement>
    {
        public int Index { get; set; }
        public Bitmap? Image { get; set; }
        public string? ImagePath { get; set; }
        public Bitmap? ImageSrc { get; set; }
        public int Y1 { get; set; }
        public int Y2 { get; set; }
        public int Width { get; set; }
        /// <summary>
        /// 原石图标所在Y坐标
        /// </summary>
        public int PrimogemsY1 { get; set; }
        public int PrimogemsY2 { get; set; }

        public string? OcrText { get; set; }

        public OcrResult? OcrResult { get; set; }

        /// <summary>
        /// 成就名称：位于图片的左上 !当前识别不准没啥用
        /// </summary>
        public string? OcrAchievementName { get; set; }
        /// <summary>
        /// 成就描述：位于图片的左下 !当前识别不准没啥用
        /// </summary>
        public string? OcrAchievementDesc { get; set; }
        /// <summary>
        /// 成就结果：位于图片的右中，穿过中轴线
        /// </summary>
        public string? OcrAchievementResult { get; set; }
        /// <summary>
        /// 成就完成时间：位于图片的右下，接近底部
        /// </summary>
        public string? OcrAchievementFinshDate { get; set; }

        /// <summary>
        /// 位于左侧的文字，用于识别与比较相似度
        /// </summary>
        public string? OcrLeftText { get; set; }

        public ExistAchievement? Match { get; set; }

        public OcrAchievement()
        {
        }

        public OcrAchievement(Bitmap image, string imagePath)
        {
            this.Image = image;
            this.ImagePath = imagePath;
        }

        public OcrAchievement Clone()
        {
            return new OcrAchievement
            {
                Index = Index,
                ImageSrc = ImageSrc,
                Y1 = Y1,
                Y2 = Y2,
                Width = Width,
                PrimogemsY1 = PrimogemsY1,
                PrimogemsY2 = PrimogemsY2,
            };
        }

        public async Task<string> OcrAsync(OcrEngine engine)
        {
            double horizontalY = this.Image!.Height * 1.0 / 2;
            double margin = horizontalY / 4; // 用于边缘容错
            double verticalX = this.Image.Width * 1.0 / 2;
            OcrResult ocrResult = await OcrHelper.RecognizeAsync(this.ImagePath!, engine);

            // 完整结果
            foreach (OcrLine? line in ocrResult.Lines)
            {
                this.OcrText += line.Concat() + Environment.NewLine;
            }

            // 严格识别不到，就默认第一行
            if (string.IsNullOrEmpty(this.OcrAchievementName) && ocrResult.Lines.Count > 0)
            {
                this.OcrAchievementName = ocrResult.Lines[0].Concat();
            }

            // 先找出左侧的两个识别区域
            List<OcrLine> leftLines = ocrResult.Lines
                .Where(line => line.Words[0].BoundingRect.Left < verticalX)
                .OrderBy(line => line.Words[0].BoundingRect.Top).ToList();
            foreach (OcrLine? line in leftLines)
            {
                this.OcrLeftText += line.Concat(); // 实际用于文本比较的内容
            }
            if (leftLines.Count == 2)
            {
                this.OcrAchievementName = leftLines[0].Concat(); // 左上
                this.OcrAchievementDesc = leftLines[1].Concat(); // 左下
            }
            else
            {
                Console.WriteLine("左侧区域未识别到2个结果" + leftLines.Count);
            }

            List<OcrLine> rightLines = ocrResult.Lines.Where(line => line.Words[0].BoundingRect.Left > verticalX).ToList();
            foreach (OcrLine? line in rightLines)
            {
                Rect firstRect = line.Words[0].BoundingRect;
                string lineStr = line.Concat();

                if ("达成".Equals(lineStr) || (firstRect.Top < horizontalY && firstRect.Bottom > horizontalY))
                {
                    this.OcrAchievementResult = lineStr; // 右中 // 98 远海牧人的宝藏 失败
                }
                else if (firstRect.Top > horizontalY)
                {
                    this.OcrAchievementFinshDate = lineStr.Replace(" ", "").Replace("／", "/"); // 右下 去空格
                }
            }

            // 严格识别不到，就默认第一行
            if (string.IsNullOrEmpty(this.OcrAchievementName) && ocrResult.Lines.Count > 0)
            {
                this.OcrAchievementName = ocrResult.Lines[0].Concat();
            }
            return this.OcrText!;
        }
    }
}
