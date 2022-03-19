using DGP.Genshin;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
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
            get => progress;
            set => SetProperty(ref progress, value, nameof(Progress));
        }
        public double ProgressMin
        {
            get => progressMin;
            set => SetProperty(ref progressMin, value, nameof(ProgressMin));
        }
        public double ProgressMax
        {
            get => progressMax;
            set => SetProperty(ref progressMax, value, nameof(ProgressMax));
        }
        public bool IsRunAsAdmin
        {
            get => App.IsElevated;
        }

        public bool WindowHandleFound
        {
            get => windowHandleFound;
            set => SetProperty(ref windowHandleFound, value);
        }

        public ICommand RestartElevatedCommand { get; }
        public ICommand CloseUICommand { get; }

        public AchievementExporterViewModel()
        {
            CheckForGameExistAsync(cancellationTokenSource.Token).Forget();
            RestartElevatedCommand = new RelayCommand(App.RestartAsElevated);
            CloseUICommand = new RelayCommand(cancellationTokenSource.Cancel);
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
                    WindowHandleFound = ProcessExists("YuanShen") || ProcessExists("GenshinImpact");
                    await Task.Delay(TimeSpan.FromSeconds(2), token).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {

            }
        }
    }
}
