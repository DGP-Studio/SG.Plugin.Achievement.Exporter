using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DGP.Genshin;
using Microsoft.VisualStudio.Threading;
using Snap.Core.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Achievement.Exporter.Plugin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class AchievementExporterViewModel : ObservableObject
    {
        private readonly CancellationTokenSource cancellationTokenSource = new();

        private double progress = 0d;
        private double progressMin = 0d;
        public double progressMax = 100d;
        private bool windowHandleFound = false;

        public double Progress
        {
            get => this.progress;
            set => this.SetProperty(ref this.progress, value, nameof(this.Progress));
        }
        public double ProgressMin
        {
            get => this.progressMin;
            set => this.SetProperty(ref this.progressMin, value, nameof(this.ProgressMin));
        }
        public double ProgressMax
        {
            get => this.progressMax;
            set => this.SetProperty(ref this.progressMax, value, nameof(this.ProgressMax));
        }
        public bool IsRunAsAdmin
        {
            get => App.IsElevated;
        }

        public bool WindowHandleFound
        {
            get => this.windowHandleFound;
            set => this.SetProperty(ref this.windowHandleFound, value);
        }

        public ICommand RestartElevatedCommand { get; }
        public ICommand CloseUICommand { get; }

        public AchievementExporterViewModel()
        {
            this.CheckForGameExistAsync(this.cancellationTokenSource.Token).Forget();
            this.RestartElevatedCommand = new RelayCommand(App.RestartAsElevated);
            this.CloseUICommand = new RelayCommand(this.cancellationTokenSource.Cancel);
        }

        public async Task CheckForGameExistAsync(CancellationToken token)
        {
            bool ProcessExists(string name)
            {
                return Process.GetProcessesByName(name).Any();
            }

            try
            {
                //haven't cancelled
                while (!token.IsCancellationRequested)
                {
                    this.WindowHandleFound = ProcessExists("YuanShen") || ProcessExists("GenshinImpact");
                    await Task.Delay(TimeSpan.FromSeconds(2), token).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {

            }
        }
    }
}
