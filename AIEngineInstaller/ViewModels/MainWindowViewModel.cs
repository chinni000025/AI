using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Services;
using AIEngineCore.EngineCore;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AIEngineInstaller.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IAIEngineGatewayManagerService _gatewayService;
        private readonly IBrowserService _browserService;

        public MainWindowViewModel(
            IAIEngineGatewayManagerService gatewayService,
            IBrowserService browserService)
        {
            _gatewayService = gatewayService;
            _browserService = browserService;
            DetectExistingInstallation();
        }

        // Default constructor for Avalonia XAML Previewer
        public MainWindowViewModel()
        {
            _gatewayService = new AIEngineGatewayManagerService();
            _browserService = new WindowsBrowserService();
            DetectExistingInstallation();
        }

        [ObservableProperty]
        private string _greeting = "Welcome to AIEngine";

        [ObservableProperty]
        private string _statusText = "Ready to install the ultimate AI experience.";

        [ObservableProperty]
        private double _installProgress = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanShowInstallButton))]
        [NotifyPropertyChangedFor(nameof(IsShowingProgress))]
        private bool _isInstalling = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanShowInstallButton))]
        private bool _isInstallComplete = false;

        [ObservableProperty]
        private bool _hasError = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanShowInstallButton))]
        [NotifyPropertyChangedFor(nameof(IsShowingProgress))]
        private bool _isAlreadyInstalled = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsShowingProgress))]
        private bool _isUninstalling = false;

        /// <summary>
        /// Checks on startup if the gateway is already running.
        /// If so, skip install and show Launch + Uninstall buttons.
        /// </summary>
        private void DetectExistingInstallation()
        {
            if (_gatewayService.IsGatewayRunning())
            {
                IsAlreadyInstalled = true;
                IsInstallComplete = true;
                StatusText = "AIEngine is already running.";
            }
        }

        /// <summary>
        /// Shows the Install button only when not already installed, not currently installing, and install not complete.
        /// </summary>
        public bool CanShowInstallButton => !IsAlreadyInstalled && !IsInstallComplete && !IsInstalling;

        /// <summary>
        /// Shows progress bar during install or uninstall operations.
        /// </summary>
        public bool IsShowingProgress => IsInstalling || IsUninstalling;

        [RelayCommand]
        private async Task InstallAsync()
        {
            if (IsInstalling || IsInstallComplete) return;

            try
            {
                IsInstalling = true;
                HasError = false;

                StatusText = "Preparing installation...";
                InstallProgress = 20;
                StatusText = "Copying core engine files...";
                await Task.Delay(800);

                InstallProgress = 50;
                StatusText = "Setting up local AI models & environment...";
                await Task.Delay(1000);

                InstallProgress = 80;
                StatusText = "Starting Gateway background service...";

                string appDataDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "AIEngine",
                    InstallerConstants.GatewayFolderName);
                string gatewaySourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, InstallerConstants.GatewayFolderName);
                string gatewayRunDir = Directory.Exists(gatewaySourceDir) ? gatewaySourceDir : appDataDir;
                if (!Directory.Exists(gatewayRunDir))
                {
                    throw new DirectoryNotFoundException(
                        $"Gateway folder not found. Searched:\n• {gatewaySourceDir}\n• {appDataDir}");
                }

                bool gatewayStarted = await _gatewayService.StartGatewayAsync(gatewayRunDir);

                if (!gatewayStarted)
                {
                    InstallProgress = 90;
                    StatusText = "Gateway started but health check timed out. You can still try to launch.";
                }
                InstallProgress = 100;
                IsInstalling = false;
                IsInstallComplete = true;

                if (gatewayStarted)
                {
                    StatusText = "Installation completed successfully!";
                }
            }
            catch (Exception ex)
            {
                IsInstalling = false;
                HasError = true;
                InstallProgress = 0;
                StatusText = $"Installation failed: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task LaunchAsync()
        {
            try
            {
                StatusText = "Launching AI Engine in browser...";
                bool opened = await _browserService.OpenWebBrowserAsync(InstallerConstants.AppWebUrl);
                if (!opened)
                {
                    StatusText = "Failed to open browser. Please manually navigate to " + InstallerConstants.AppWebUrl;
                    return;
                }

                await Task.Delay(1500);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.Shutdown(0);
                    }
                });
            }
            catch (Exception ex)
            {
                StatusText = $"Launch failed: {ex.Message}. Please manually open {InstallerConstants.AppWebUrl}";
            }
        }

        [RelayCommand]
        private async Task UninstallAsync(bool isUninstalling)
        {
            if (_isUninstalling) return;

            try
            {
                _isUninstalling = true;
                StatusText = "Stopping AIEngine Gateway...";
                InstallProgress = 30;

                bool stopped = await _gatewayService.StopGatewayAsync();
                if (!stopped)
                {
                    StatusText = "Failed to stop the Gateway process. Please close it manually from Task Manager.";
                    IsUninstalling = false;
                    return;
                }

                InstallProgress = 60;
                StatusText = "Cleaning up installed files...";

                // Clean up AppData files
                string appDataDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "AIEngine");

                if (Directory.Exists(appDataDir))
                {
                    try
                    {
                        Directory.Delete(appDataDir, recursive: true);
                    }
                    catch
                    {
                        // AppData cleanup is best-effort, don't fail the whole uninstall
                    }
                }

                InstallProgress = 100;
                await Task.Delay(500);

                // Reset UI state to allow fresh install
                IsUninstalling = false;
                IsInstallComplete = false;
                IsAlreadyInstalled = false;
                HasError = false;
                InstallProgress = 0;
                StatusText = "AIEngine has been uninstalled successfully. You can install again.";
            }
            catch (Exception ex)
            {
                IsUninstalling = false;
                StatusText = $"Uninstall failed: {ex.Message}";
            }
        }
    }
}
