using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Windows.Media.Ocr;

namespace Achievement.Exporter.Plugin
{
    internal class AchievementCtrl
    {
        public const int CaptureInterval = 20;
        public const string HotkeyStop = "F11";

        public event EventHandler<(AchievementProcessing, double, double, double)>? ProgressUpdated;
        public event EventHandler<AchievementException>? ExceptionCatched;
        public event EventHandler<AchievementMessage>? MessageCatched;

        private static string userDataPath = null!;
        private static string imgPagePath = null!;
        private static string imgSectionPath = null!;

        private readonly ImageCapture capture = new();
        private readonly GenshinWindow window = new();

        private int x, y, w, h;
        private bool stopFlag = false;
        internal PaimonMoeJson paimonMoeJson = PaimonMoeJson.Builder();

        public bool GenshinStatus => window.FindWindowHandle();

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

        static AchievementCtrl()
        {
            userDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AchievementData");
            imgPagePath = Path.Combine(userDataPath, "pages");
            imgSectionPath = Path.Combine(userDataPath, "sections");
        }

        public AchievementCtrl()
        {
        }

        public void Setup()
        {
            try
            {
                RegisterHotKey(HotkeyStop);
            }
            catch (Exception ex)
            {
                PrintMsg(ex.Message);
                ExceptionCatched?.Invoke(this, new("热键注册失败", ex));
            }
        }

        public async Task StartAsync()
        {
            try
            {
                if (!GenshinStatus)
                {
                    PrintMsg("未找到原神进程，请先启动原神！");
                    Processing = AchievementProcessing.None;
                    return;
                }
                capture.Start();

                stopFlag = false;

                // 1.切换到原神窗口
                PrintMsg("切换到原神窗口");
                window.Focus();
                await Task.Delay(200);

                // 2. 定位截图选区
                Rectangle rc = window.GetSize();
                x = (int)Math.Ceiling(rc.X * PrimaryScreen.ScaleX);
                y = (int)Math.Ceiling(rc.Y * PrimaryScreen.ScaleY);
                w = (int)Math.Ceiling(rc.Width * PrimaryScreen.ScaleX);
                h = (int)Math.Ceiling(rc.Height * PrimaryScreen.ScaleY);
                Bitmap ysWindowPic = capture.Capture(x, y, w, h);

                // 使用新的坐标
                Rectangle rect = ImageRecognition.CalculateCatchArea(ysWindowPic);
                PrintMsg("已定位成就栏位置");
                x += rect.X;
                y += rect.Y;
                w = rect.Width + 2;
                h = rect.Height;

                Application.Current.MainWindow.Focus();
                ContentDialogResult result = await new PreviewCaptureAreaDialog(capture.Capture(x, y, w, h).ToBitmapImage()).ShowAsync();
                if (result is not ContentDialogResult.Primary)
                {
                    Processing = AchievementProcessing.None;
                    return;
                }
                window.Focus();
                await Task.Delay(200);

                PrintMsg($"0.5s后开始自动滚动截图，按{HotkeyStop}终止滚动！");
                await Task.Delay(500);
                window.Click(x, y, h);

                IOUtils.CreateFolder(userDataPath);
                IOUtils.CreateFolder(imgPagePath);
                IOUtils.DeleteFolder(imgPagePath);

                paimonMoeJson = PaimonMoeJson.Builder();

                await Task.Run(async () =>
                {
                    // 3. 滚动截图
                    int rowIn = 0, rowOut = 0, n = 0;

                    while (rowIn < 15 && rowOut < 15)
                    {
                        if (stopFlag)
                        {
                            PrintMsg($"滚动已经终止");
                            break;
                        }
                        try
                        {
                            Bitmap pagePic = capture.Capture(x, y, w, h);
                            if (n % CaptureInterval == 0)
                            {
                                pagePic.Save(Path.Combine(imgPagePath, n + ".png"));
                                //PrintMsg($"{n}：截图并保存");
                            }

                            Bitmap onePixHightPic = capture.Capture(x, y + h - 20, w, 1); // 截取一个1pix的长条
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
                            PrintMsg(ex.Message + Environment.NewLine + ex.StackTrace);
                        }

                        window.Click(x, y, h);
                        window.MouseWheelDown();
                        n++;
                    }
                    if (!stopFlag)
                    {
                        Bitmap lastPagePic = capture.Capture(x, y, w, h);
                        lastPagePic.Save(Path.Combine(imgPagePath, ++n + ".png"));
                        PrintMsg("滚动截图完成");

                        // 4. 分割截图
                        PrintMsg("截图处理中...");
                        PageToSection();
                        PrintMsg("截图处理完成");

                        // 5. OCR
                        List<OcrAchievement> list = LoadImgSection();
                        PrintMsg("文字识别中...");
                        await OcrAsync(list);
                        PrintMsg("文字识别完成");
                        await Task.Delay(100);
                        PrintMsg("成就匹配中...");
                        Matching(list);
                        PrintMsg("成就匹配完成");
                        PrintMsg($"你可以点击对应的按钮导出成就啦~");
                    }
                });

                capture.Stop();
            }
            catch (Exception e)
            {
                ExceptionCatched?.Invoke(this, new("错误", e));
            }
            Processing = AchievementProcessing.None;
        }

        protected private virtual void OnProgressUpdated()
        {
            ProgressUpdated?.Invoke(this, (Processing, Progress, ProgressMin, ProgressMax));
        }

        /// <summary>
        /// 读取截图并切片
        /// </summary>
        private void PageToSection()
        {
            IOUtils.CreateFolder(imgSectionPath);
            IOUtils.DeleteFolder(imgSectionPath);

            DirectoryInfo dir = new(imgPagePath);
            FileInfo[] fileInfo = dir.GetFiles();

            Processing = AchievementProcessing.PageToSection;
            ProgressMax = fileInfo.Length;
            Progress = 0d;
            foreach (FileInfo item in fileInfo)
            {
                Bitmap imgPage = ImageUtils.FromFile(item.FullName);
                List<Bitmap> list = ImageRecognition.Split(imgPage);
                for (int i = 0; i < list.Count; i++)
                {
                    //list[i].Binary();
                    list[i].Save(Path.Combine(imgSectionPath, item.Name + "_" + i + ".png"));
                }
                //PrintMsg($"{item.Name}切片完成");
                Progress++;
            }
        }

        private List<OcrAchievement> LoadImgSection()
        {
            List<OcrAchievement> list = new();
            DirectoryInfo dir = new(imgSectionPath);
            FileInfo[] fileInfo = dir.GetFiles();

            foreach (FileInfo item in fileInfo)
            {
                OcrAchievement achievement = new();
                achievement.Image = ImageUtils.FromFile(item.FullName);
                achievement.ImagePath = item.FullName;
                list.Add(achievement);
            }
            return list;
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
                Trace.WriteLine(r);
                Progress++;
            }
        }

        private void Matching(List<OcrAchievement> achievementList)
        {
            Processing = AchievementProcessing.Matching;
            ProgressMax = achievementList.Count;
            Progress = 0d;
            foreach (OcrAchievement a in achievementList)
            {
                paimonMoeJson.Matching("天地万象", a);
                Progress++;
            }
        }

        private void PrintMsg(string message, AchievementMessageLevel? level = null)
        {
            string msg = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Trace.WriteLine(msg);
            MessageCatched?.Invoke(this, new(level ?? AchievementMessageLevel.Info, msg));
        }

        #region [HotKey]
        private static Hotkey hotkey = null!;
        private static HotkeyHook hotkeyHook = null!;

        public void RegisterHotKey(string hotkeyStr)
        {
            if (string.IsNullOrEmpty(hotkeyStr))
            {
                UnregisterHotKey();
                return;
            }

            hotkey = new Hotkey(hotkeyStr);

            hotkeyHook?.Dispose();
            hotkeyHook = new HotkeyHook();
            hotkeyHook.KeyPressed += (_, _) => stopFlag = true;
            hotkeyHook.RegisterHotKey(hotkey.ModifierKey, hotkey.Key);
        }

        public void UnregisterHotKey()
        {
            if (hotkeyHook != null)
            {
                hotkeyHook.UnregisterHotKey();
                hotkeyHook.Dispose();
            }
        }
        #endregion
    }
}
