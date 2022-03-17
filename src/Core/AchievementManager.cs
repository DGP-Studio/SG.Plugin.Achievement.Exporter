using Achievement.Exporter.Plugin.Core.Hotkey;
using Achievement.Exporter.Plugin.Helper;
using Achievement.Exporter.Plugin.Model;
using Achievement.Exporter.Plugin.View;
using DGP.Genshin;
using DGP.Genshin.Helper;
using ModernWpf.Controls;
using Snap.Core.Logging;
using Snap.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Media.Ocr;

namespace Achievement.Exporter.Plugin.Core
{
    internal class AchievementManager
    {
        public const int CaptureInterval = 20;
        public const string StopHotkey = "F11";

        public event EventHandler<(AchievementProcessing, double, double, double)>? ProgressUpdated;
        public event EventHandler<AchievementException>? ExceptionCatched;
        public event EventHandler<AchievementMessage>? MessageCatched;

        private static readonly string userDataPath = PathContext.Locate("AchievementData");
        private static readonly string imgPagePath = PathContext.Locate(userDataPath, "pages");
        private static readonly string imgSectionPath = PathContext.Locate(userDataPath, "sections");

        private readonly ImageCapturing capturing = new();
        private readonly GenshinWindow window = new();

        //原神窗口位置
        private int left, top, width, height;

        /// <summary>
        /// 滚动截图标识
        /// </summary>
        private bool capturingStopFlag = false;
        internal PaimonMoeJson paimonMoeJson = PaimonMoeJson.Build();

        public bool GenshinStatus
        {
            get => window.FindWindowHandle();
        }

        private AchievementProcessing processing = AchievementProcessing.None;
        public AchievementProcessing Processing
        {
            get => processing;
            set
            {
                processing = value;
                OnProgressUpdated();
            }
        }

        private double progress = 0d;
        public double Progress
        {
            get => progress;
            set
            {
                progress = value;
                OnProgressUpdated();
            }
        }

        private double progressMin = 0d;
        public double ProgressMin
        {
            get => progressMin;
            set
            {
                progressMin = value;
                OnProgressUpdated();
            }
        }

        private double progressMax = 100d;
        public double ProgressMax
        {
            get => progressMax;
            set
            {
                progressMax = value;
                OnProgressUpdated();
            }
        }

        public void Initialize()
        {
            try
            {
                RegisterHotKey(StopHotkey);
            }
            catch (Exception ex)
            {
                Notify(ex.Message);
                ExceptionCatched?.Invoke(this, new("热键注册失败", ex));
            }
        }

        public async Task StartAsync()
        {
            try
            {
                if (!GenshinStatus)
                {
                    Notify("未找到原神进程，请先启动原神！");
                    Processing = AchievementProcessing.None;
                    return;
                }
                using (capturing.Session())
                {
                    capturingStopFlag = false;

                    //切换到原神窗口
                    Notify("切换到原神窗口");
                    window.Focus();
                    await Task.Delay(200);

                    Bitmap genshinWindowCapture = RenderGenshinWindowCapture();

                    //使用新的坐标
                    LocateAchievementArea(genshinWindowCapture);

                    App.Current.MainWindow.Focus();
                    BitmapImage preview = capturing.Sample(left, top, width, height).ToBitmapImage();
                    ContentDialogResult result = await new PreviewCaptureAreaDialog(preview).ShowAsync();
                    if (result is not ContentDialogResult.Primary)
                    {
                        Processing = AchievementProcessing.None;
                        return;
                    }
                    window.Focus();
                    await Task.Delay(200);

                    Notify($"0.5s后开始自动滚动截图，按{StopHotkey}终止滚动！");
                    await Task.Delay(500);
                    window.Click(left, top, height);

                    PathContext.CreateFolderOrIgnore(userDataPath);
                    PathContext.CreateFolderOrIgnore(imgPagePath);
                    PathContext.DeleteFolderOrIgnore(imgPagePath);

                    paimonMoeJson = PaimonMoeJson.Build();
                    await ScrollCaptureAsync();
                }
            }
            catch (Exception e)
            {
                ExceptionCatched?.Invoke(this, new("错误", e));
            }
            Processing = AchievementProcessing.None;
        }

        private void LocateAchievementArea(Bitmap genshinWindowCapture)
        {
            Rectangle rect = ImageRecognition.CalculateSampleArea(genshinWindowCapture);
            Notify("已定位成就栏位置");
            left += rect.X;
            top += rect.Y;
            width = rect.Width + 2;
            height = rect.Height;
        }

        private async Task ScrollCaptureAsync()
        {
            await Task.Run(async () =>
            {
                // 3. 滚动截图
                int rowIn = 0, rowOut = 0, n = 0;

                while (rowIn < 15 && rowOut < 15)
                {
                    if (capturingStopFlag)
                    {
                        Notify("滚动已经终止");
                        break;
                    }
                    try
                    {
                        Bitmap pagePic = capturing.Sample(left, top, width, height);
                        if (n % CaptureInterval == 0)
                        {
                            pagePic.Save(Path.Combine(imgPagePath, n + ".png"));
                        }

                        Bitmap onePixHightPic = capturing.Sample(left, top + height - 20, width, 1); // 截取一个1pix的长条
                        if (ImageRecognition.IsInRow(onePixHightPic))
                        {
                            rowIn++;
                            rowOut = 0;
                        }
                        else
                        {
                            rowIn = 0;
                            rowOut++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Notify(ex.Message + Environment.NewLine + ex.StackTrace);
                    }

                    window.Click(left, top, height);
                    window.MouseWheelDown();
                    n++;
                }
                if (!capturingStopFlag)
                {
                    Bitmap lastPagePic = capturing.Sample(left, top, width, height);
                    lastPagePic.Save(Path.Combine(imgPagePath, ++n + ".png"));
                    Notify("滚动截图完成");

                    //分割截图
                    Notify("截图处理中...");
                    PageToSection();
                    Notify("截图处理完成");

                    //OCR
                    List<OcrAchievement> list = LoadImageSection();
                    Notify("文字识别中...");
                    await OcrAsync(list);
                    Notify("文字识别完成");

                    await Task.Delay(100);
                    Notify("成就匹配中...");
                    Match(list);
                    Notify("成就匹配完成");
                    Notify($"你可以点击对应的按钮导出成就啦~");
                }
            });
        }

        /// <summary>
        /// 捕获原神窗口截图
        /// </summary>
        /// <returns></returns>
        private Bitmap RenderGenshinWindowCapture()
        {
            //定位截图选区
            Rectangle genshinRect = window.GetSize();
            left = (int)Math.Ceiling(genshinRect.X * PrimaryScreen.ScaleX);
            top = (int)Math.Ceiling(genshinRect.Y * PrimaryScreen.ScaleY);
            width = (int)Math.Ceiling(genshinRect.Width * PrimaryScreen.ScaleX);
            height = (int)Math.Ceiling(genshinRect.Height * PrimaryScreen.ScaleY);

            return capturing.Sample(left, top, width, height);
        }

        private void OnProgressUpdated()
        {
            ProgressUpdated?.Invoke(this, (Processing, Progress, ProgressMin, ProgressMax));
        }

        /// <summary>
        /// 读取截图并切片
        /// </summary>
        private void PageToSection()
        {
            PathContext.CreateFolderOrIgnore(imgSectionPath);
            PathContext.DeleteFolderOrIgnore(imgSectionPath);

            DirectoryInfo dir = new(imgPagePath);
            FileInfo[] fileInfo = dir.GetFiles();

            Processing = AchievementProcessing.PageToSection;
            ProgressMax = fileInfo.Length;
            Progress = 0d;
            foreach (FileInfo item in fileInfo)
            {
                Bitmap imgPage = BitmapExtensions.FromFile(item.FullName);
                List<Bitmap> list = ImageRecognition.Split(imgPage);
                for (int i = 0; i < list.Count; i++)
                {
                    //list[i].Binary();
                    list[i].Save(Path.Combine(imgSectionPath, item.Name + "_" + i + ".png"));
                }
                Progress++;
            }
        }

        private List<OcrAchievement> LoadImageSection()
        {
            return new DirectoryInfo(imgSectionPath)
                .GetFiles()
                .Select(file => new OcrAchievement(BitmapExtensions.FromFile(file.FullName), file.FullName))
                .ToList();
        }

        private async Task OcrAsync(List<OcrAchievement> achievementList)
        {
            OcrEngine engine = OcrEngine.TryCreateFromLanguage(new("zh-Hans-CN"));

            Processing = AchievementProcessing.Ocr;
            ProgressMax = achievementList.Count;
            Progress = 0d;
            foreach (OcrAchievement a in achievementList)
            {
                string r = await a.OcrAsync(engine);
                this.Log(r);
                Progress++;
            }
        }

        private void Match(List<OcrAchievement> achievementList)
        {
            Processing = AchievementProcessing.Matching;
            ProgressMax = achievementList.Count;
            Progress = 0d;
            foreach (OcrAchievement achievement in achievementList)
            {
                paimonMoeJson.Match("天地万象", achievement);
                Progress++;
            }
        }

        private void Notify(string message, AchievementMessageLevel? level = null)
        {
            this.Log(message);
            MessageCatched?.Invoke(this, new(level ?? AchievementMessageLevel.Info, message));
        }

        #region HotKey
        private static Hotkey.Hotkey hotkey = null!;
        private static HotkeyHook hotkeyHook = null!;

        public void RegisterHotKey(string hotkeyStr)
        {
            if (string.IsNullOrEmpty(hotkeyStr))
            {
                UnregisterHotKey();
                return;
            }

            hotkey = new(hotkeyStr);

            hotkeyHook?.Dispose();
            hotkeyHook = new HotkeyHook();
            hotkeyHook.KeyPressed += (_, _) => capturingStopFlag = true;
            hotkeyHook.Register(hotkey.ModifierKey, hotkey.Key);
        }

        public void UnregisterHotKey()
        {
            if (hotkeyHook != null)
            {
                hotkeyHook.Unregister();
                hotkeyHook.Dispose();
            }
        }
        #endregion
    }
}
