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

        // 原神窗口位置
        private int left, top, width, height;

        /// <summary>
        /// 滚动截图标识
        /// </summary>
        private bool capturingStopFlag = false;
        internal PaimonMoeJson paimonMoeJson = PaimonMoeJson.Build();

        public bool GenshinStatus
        {
            get => this.window.FindWindowHandle();
        }

        private AchievementProcessing processing = AchievementProcessing.None;
        public AchievementProcessing Processing
        {
            get => this.processing;
            set
            {
                this.processing = value;
                this.OnProgressUpdated();
            }
        }

        private double progress = 0d;
        public double Progress
        {
            get => this.progress;
            set
            {
                this.progress = value;
                this.OnProgressUpdated();
            }
        }

        private double progressMin = 0d;
        public double ProgressMin
        {
            get => this.progressMin;
            set
            {
                this.progressMin = value;
                this.OnProgressUpdated();
            }
        }

        private double progressMax = 100d;
        public double ProgressMax
        {
            get => this.progressMax;
            set
            {
                this.progressMax = value;
                this.OnProgressUpdated();
            }
        }

        public void Initialize()
        {
            try
            {
                this.RegisterHotKey(StopHotkey);
            }
            catch (Exception ex)
            {
                this.Notify(ex.Message);
                ExceptionCatched?.Invoke(this, new("热键注册失败", ex));
            }
        }

        public async Task StartAsync()
        {
            try
            {
                if (!this.GenshinStatus)
                {
                    this.Notify("未找到原神进程，请先启动原神！");
                    this.Processing = AchievementProcessing.None;
                    return;
                }
                using (this.capturing.Session())
                {
                    this.capturingStopFlag = false;

                    // 切换到原神窗口
                    this.Notify("切换到原神窗口");
                    this.window.Focus();
                    await Task.Delay(200);

                    Bitmap genshinWindowCapture = this.RenderGenshinWindowCapture();

                    // 使用新的坐标
                    this.LocateAchievementArea(genshinWindowCapture);

                    App.Current.MainWindow.Focus();
                    BitmapImage preview = this.capturing.Sample(this.left, this.top, this.width, this.height).ToBitmapImage();
                    ContentDialogResult result = await new PreviewCaptureAreaDialog(preview).ShowAsync();
                    if (result is not ContentDialogResult.Primary)
                    {
                        this.Processing = AchievementProcessing.None;
                        return;
                    }
                    this.window.Focus();
                    await Task.Delay(200);

                    this.Notify($"开始自动滚动截图，按{StopHotkey}终止滚动！");
                    await Task.Delay(500);
                    this.window.Click(this.left, this.top, this.height);

                    PathContext.CreateFolderOrIgnore(userDataPath);
                    PathContext.CreateFolderOrIgnore(imgPagePath);
                    PathContext.DeleteFolderOrIgnore(imgPagePath);
                    PathContext.CreateFolderOrIgnore(imgPagePath);

                    this.paimonMoeJson = PaimonMoeJson.Build();
                    await this.ScrollCaptureAsync();
                }
            }
            catch (Exception e)
            {
                ExceptionCatched?.Invoke(this, new("错误", e));
            }
            this.Processing = AchievementProcessing.None;
        }

        private void LocateAchievementArea(Bitmap genshinWindowCapture)
        {
            Rectangle rect = ImageRecognition.CalculateSampleArea(genshinWindowCapture);
            this.Notify("已定位成就栏位置");
            this.left += rect.X;
            this.top += rect.Y;
            this.width = rect.Width + 2;
            this.height = rect.Height;
        }

        private async Task ScrollCaptureAsync()
        {
            await Task.Run(async () =>
            {
                // 滚动截图
                int rowIn = 0, rowOut = 0, n = 0;

                while (rowIn < 15 && rowOut < 15)
                {
                    if (this.capturingStopFlag)
                    {
                        this.Notify("滚动已经终止");
                        break;
                    }
                    try
                    {
                        Bitmap pagePic = this.capturing.Sample(this.left, this.top, this.width, this.height);
                        if (n % CaptureInterval == 0)
                        {
                            pagePic.Save(Path.Combine(imgPagePath, n + ".png"));
                        }

                        Bitmap onePixHightPic = this.capturing.Sample(this.left, this.top + this.height - 20, this.width, 1); // 截取一个1pix的长条
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
                        this.Notify(ex.Message + Environment.NewLine + ex.StackTrace);
                    }

                    this.window.Click(this.left, this.top, this.height);
                    this.window.MouseWheelDown();
                    n++;
                }
                if (!this.capturingStopFlag)
                {
                    Bitmap lastPagePic = this.capturing.Sample(this.left, this.top, this.width, this.height);
                    lastPagePic.Save(Path.Combine(imgPagePath, ++n + ".png"));
                    this.Notify("滚动截图完成");

                    // 分割截图
                    this.Notify("截图处理中...");
                    this.PageToSection();
                    this.Notify("截图处理完成");

                    // OCR
                    List<OcrAchievement> list = this.LoadImageSection();
                    this.Notify("文字识别中...");
                    await this.OcrAsync(list);
                    this.Notify("文字识别完成");

                    await Task.Delay(100);
                    this.Notify("成就匹配中...");
                    this.Match(list);
                    this.Notify("成就匹配完成");
                    this.Notify($"你可以点击对应的按钮导出成就啦~");
                }
            });
        }

        /// <summary>
        /// 捕获原神窗口截图
        /// </summary>
        private Bitmap RenderGenshinWindowCapture()
        {
            // 定位截图选区
            Rectangle genshinRect = this.window.GetSize();
            this.left = (int)Math.Ceiling(genshinRect.X * PrimaryScreen.ScaleX);
            this.top = (int)Math.Ceiling(genshinRect.Y * PrimaryScreen.ScaleY);
            this.width = (int)Math.Ceiling(genshinRect.Width * PrimaryScreen.ScaleX);
            this.height = (int)Math.Ceiling(genshinRect.Height * PrimaryScreen.ScaleY);

            return this.capturing.Sample(this.left, this.top, this.width, this.height);
        }

        private void OnProgressUpdated()
        {
            ProgressUpdated?.Invoke(this, (this.Processing, this.Progress, this.ProgressMin, this.ProgressMax));
        }

        /// <summary>
        /// 读取截图并切片
        /// </summary>
        private void PageToSection()
        {
            PathContext.CreateFolderOrIgnore(imgSectionPath);
            PathContext.DeleteFolderOrIgnore(imgSectionPath);
            PathContext.CreateFolderOrIgnore(imgSectionPath);

            DirectoryInfo dir = new(imgPagePath);
            FileInfo[] fileInfo = dir.GetFiles();

            this.Processing = AchievementProcessing.PageToSection;
            this.ProgressMax = fileInfo.Length;
            this.Progress = 0d;
            foreach (FileInfo item in fileInfo)
            {
                Bitmap imgPage = BitmapExtensions.FromFile(item.FullName);
                List<Bitmap> list = ImageRecognition.Split(imgPage);
                for (int i = 0; i < list.Count; i++)
                {
                    //list[i].Binary();
                    list[i].Save(Path.Combine(imgSectionPath, item.Name + "_" + i + ".png"));
                }
                this.Progress++;
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

            this.Processing = AchievementProcessing.Ocr;
            this.ProgressMax = achievementList.Count;
            this.Progress = 0d;
            foreach (OcrAchievement a in achievementList)
            {
                string r = await a.OcrAsync(engine);
                this.Log(r);
                this.Progress++;
            }
        }

        private void Match(List<OcrAchievement> achievementList)
        {
            this.Processing = AchievementProcessing.Matching;
            this.ProgressMax = achievementList.Count;
            this.Progress = 0d;
            foreach (OcrAchievement achievement in achievementList)
            {
                this.paimonMoeJson.Match("天地万象", achievement);
                this.Progress++;
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
                this.UnregisterHotKey();
                return;
            }

            hotkey = new(hotkeyStr);

            hotkeyHook?.Dispose();
            hotkeyHook = new HotkeyHook();
            hotkeyHook.KeyPressed += (_, _) => this.capturingStopFlag = true;
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
