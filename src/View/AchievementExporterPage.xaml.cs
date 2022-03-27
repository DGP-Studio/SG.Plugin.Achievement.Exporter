using Achievement.Exporter.Plugin.Core;
using Achievement.Exporter.Plugin.ViewModel;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using System;

namespace Achievement.Exporter.Plugin.View
{
    [View(InjectAs.Transient)]
    internal partial class AchievementExporterPage : System.Windows.Controls.Page
    {
        private readonly AchievementExporterViewModel viewModel;
        internal AchievementManager manager;

        public AchievementExporterPage(AchievementExporterViewModel vm)
        {
            this.DataContext = vm;
            this.viewModel = vm;
            this.manager = new();
            this.InitializeComponent();

            Loaded += (s, e) =>
            {
                this.manager.Initialize();
            };

            this.buttonStart.Click += (s, e) =>
            {
                this.SetIsEnabled(false);
                this.logs.Clear();
                _ = this.manager.StartAsync();
            };

            this.buttonExport.Click += (s, e) =>
            {
                _ = new ExportDialog(this.manager.paimonMoeJson).ShowAsync();
            };

            this.manager.ProgressUpdated += (s, e) =>
            {
                (AchievementProcessing processing, double value, double min, double max) = e;

                this.Log($"[{processing}] {value}/{max} {value / max * 100d:0}%");

                if (processing == AchievementProcessing.None)
                {
                    this.SetIsEnabled(true);
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.viewModel.Progress = value;
                        this.viewModel.ProgressMin = min;
                        this.viewModel.ProgressMax = max;
                    });
                }
            };

            this.manager.ExceptionCatched += (s, e) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.logs.AppendText(e.ToString());
                    this.logs.AppendText(Environment.NewLine);
                    this.logs.ScrollToEnd();
                });
            };

            this.manager.MessageCatched += (s, e) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.logs.AppendText(e.ToString());
                    this.logs.AppendText(Environment.NewLine);
                    this.logs.ScrollToEnd();
                });
            };
        }

        private void SetIsEnabled(bool isEnabled)
        {
            this.Dispatcher?.Invoke(() => this.commandBar.IsEnabled = isEnabled);
        }
    }
}
