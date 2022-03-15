using DGP.Genshin;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Snap.Core.DependencyInjection;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Achievement.Exporter.Plugin
{
    [ViewModel(InjectAs.Transient)]
    internal class AchievementExporterViewModel : ObservableObject
    {
        protected DispatcherTimer DispatcherTimer = new() { Interval = new(TimeSpan.TicksPerMillisecond * 2000) };

        private double progress = 0d;
        public double Progress
        {
            get => progress;
            set => SetProperty(ref progress, value, nameof(Progress));
        }

        private double progressMin = 0d;
        public double ProgressMin
        {
            get => progressMin;
            set => SetProperty(ref progressMin, value, nameof(ProgressMin));
        }

        public double progressMax = 100d;
        public double ProgressMax
        {
            get => progressMax;
            set => SetProperty(ref progressMax, value, nameof(ProgressMax));
        }

        public bool IsRunAsAdmin
        {
            get
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new(id);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private bool windowHandleFound = false;
        public bool WindowHandleFound
        {
            get => windowHandleFound;
            set => SetProperty(ref windowHandleFound, value);
        }

        public ICommand? RestartElevatedCommand { get; }

        public AchievementExporterViewModel()
        {
            CheckGenshinImpact();
            DispatcherTimer.Tick += (s, e) => CheckGenshinImpact();
            DispatcherTimer.Start();
            RestartElevatedCommand = new AsyncRelayCommand<Button>(RestartElevatedAsync);
        }

        public void CheckGenshinImpact()
        {
            WindowHandleFound = Process.GetProcessesByName("YuanShen").Any() || Process.GetProcessesByName("GenshinImpact").Any();
        }

        public async Task RestartElevatedAsync(Button? button)
        {
            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = $"{Process.GetCurrentProcess().ProcessName}.exe",
                });
            }
            catch (Win32Exception)
            {
                return;
            }

            App.Current.Shutdown();
        }
    }
}
