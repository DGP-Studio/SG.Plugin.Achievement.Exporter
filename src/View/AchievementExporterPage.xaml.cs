using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Achievement.Exporter.Plugin
{
    [View(InjectAs.Transient)]
    public partial class AchievementExporterPage : System.Windows.Controls.Page
    {
        internal AchievementExporterViewModel ViewModel => (DataContext as AchievementExporterViewModel)!;
        internal AchievementCtrl Ctrl;

        public AchievementExporterPage()
        {
            DataContext = new AchievementExporterViewModel();
            Ctrl = new();
            InitializeComponent();

            Loaded += (s, e) =>
            {
                Ctrl.Setup();
            };

            buttonStart.Click += (s, e) =>
            {
                SetIsEnabled(false);
                logs.Clear();
                _ = Ctrl.StartAsync();
            };

            buttonExport.Click += async (s, e) =>
            {
                ContentDialogResult result = await new ExportDialog(Ctrl.paimonMoeJson).ShowAsync();

                if (result is not ContentDialogResult.Primary)
                {
                }
            };

            Ctrl.ProgressUpdated += (s, e) =>
            {
                (AchievementProcessing processing, double value, double min, double max) = e;

                Trace.WriteLine($"[{processing}] {value}/{max} {value / max * 100d:0}%");

                if (processing == AchievementProcessing.None)
                {
                    SetIsEnabled(true);
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        ViewModel.Progress = value;
                        ViewModel.ProgressMin = min;
                        ViewModel.ProgressMax = max;
                    });
                }
            };

            Ctrl.ExceptionCatched += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    logs.AppendText(e.ToString());
                    logs.AppendText(Environment.NewLine);
                    logs.ScrollToEnd();
                });
            };

            Ctrl.MessageCatched += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    logs.AppendText(e.ToString());
                    logs.AppendText(Environment.NewLine);
                    logs.ScrollToEnd();
                });
            };
        }

        private void SetIsEnabled(bool isEnabled)
        {
            Dispatcher?.Invoke(() => commandBar.IsEnabled = isEnabled);
        }
    }
}
