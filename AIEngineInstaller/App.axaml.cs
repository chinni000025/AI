using AIEngineConnectivity.Services;
using AIEngineInstaller.ViewModels;
using AIEngineInstaller.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace AIEngineInstaller
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var systemCheck = Program.Services.GetRequiredService<ISystemCheckService>();
                if (!systemCheck.CanInstallEngine())
                {
                    desktop.MainWindow = new ErrorWindow();
                }
                else
                {
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = new MainWindowViewModel(),
                    };
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}