using Achievement.Exporter.Plugin.Core;
using Achievement.Exporter.Plugin.ViewModel;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Achievement.Exporter.Plugin
{
    [View(InjectAs.Transient)]
    internal partial class AchievementExporterPage : System.Windows.Controls.Page
    {
        private readonly AchievementExporterViewModel viewModel;
        internal AchievementManager manager;

        public AchievementExporterPage(AchievementExporterViewModel vm)
        {
            DataContext = vm;
            viewModel = vm;
            manager = new();
            InitializeComponent();

            Loaded += (s, e) =>
            {
                manager.Initialize();
            };

            buttonStart.Click += (s, e) =>
            {
                SetIsEnabled(false);
                logs.Clear();
                _ = manager.StartAsync();
            };

            //buttonExport.Click += async (s, e) =>
            //{
            //    ContentDialogResult result = await new ExportDialog(manager.paimonMoeJson).ShowAsync();

            //    if (result is not ContentDialogResult.Primary)
            //    {
            //    }
            //};

            manager.ProgressUpdated += (s, e) =>
            {
                (AchievementProcessing processing, double value, double min, double max) = e;

                this.Log($"[{processing}] {value}/{max} {value / max * 100d:0}%");

                if (processing == AchievementProcessing.None)
                {
                    SetIsEnabled(true);
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        viewModel.Progress = value;
                        viewModel.ProgressMin = min;
                        viewModel.ProgressMax = max;
                    });
                }
            };

            manager.ExceptionCatched += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    logs.AppendText(e.ToString());
                    logs.AppendText(Environment.NewLine);
                    logs.ScrollToEnd();
                });
            };

            manager.MessageCatched += (s, e) =>
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
