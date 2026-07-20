using AIEngineConnectivity.Services;
using AIEngineInstaller.ViewModels;
namespace AIEngineInstaller
{
    using AIEngineInstaller.Views;
    using Avalonia;
    using Avalonia.Controls.ApplicationLifetimes;
    using Avalonia.Markup.Xaml;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;
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
                var context = Program.provider.GetRequiredService<IInstallerService>();
                var requiredContext = context.getRequiredContext();
                if (!requiredContext.CanInstallEngine())
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