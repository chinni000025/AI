using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace AIEngineInstaller.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
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

        [RelayCommand]
        private async Task InstallAsync()
        {
            if (IsInstalling || IsInstallComplete) return;

            IsInstalling = true;
            StatusText = "Preparing installation...";

            // Simulate installation process
            for (int i = 0; i <= 100; i += 5)
            {
                InstallProgress = i;

                if (i == 20) StatusText = "Copying core engine files...";
                if (i == 50) StatusText = "Setting up local AI models...";
                if (i == 80) StatusText = "Configuring environment...";
                if (i == 95) StatusText = "Finishing up...";

                await Task.Delay(1000); // Simulate work
            }

            IsInstalling = false;
            IsInstallComplete = true;
            StatusText = "Installation completed successfully!";
            InstallProgress = 100;
        }

        [RelayCommand]
        private void Launch()
        {
            // Close installer or launch app logic here
            System.Environment.Exit(0);
        }
    }
}
