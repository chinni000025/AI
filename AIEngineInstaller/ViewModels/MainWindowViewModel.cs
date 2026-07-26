namespace AIEngineInstaller.ViewModels
{
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
        }

        // Default constructor for Avalonia XAML Previewer
        public MainWindowViewModel()
        {
            _gatewayService = new AIEngineGatewayManagerService();
            _browserService = new WindowsBrowserService();
        }

        [ObservableProperty]
        private string _greeting = "Welcome to AIEngine";

        [ObservableProperty]
        private string _statusText = "Ready to install the ultimate AI experience.";

        [ObservableProperty]
        private double _installProgress = 0;

        [ObservableProperty]
        private bool _isInstalling = false;

        [ObservableProperty]
        private bool _isInstallComplete = false;

        [ObservableProperty]
        private bool _hasError = false;

        [RelayCommand]
        private async Task InstallAsync()
        {
            if (IsInstalling || IsInstallComplete) return;

            try
            {
                IsInstalling = true;
                HasError = false;
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
    }
}
