namespace AIEngineInstaller
{
    using Avalonia;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using AIEngineConnectivity.Services;
    using AIEngineCore.EngineCore;
    using AIEngineInstaller.Extensions;
    internal sealed class Program
    {
        public static IServiceProvider provider { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            var services = new ServiceCollection();

            // Register services
            services.addServices();
            provider = services.BuildServiceProvider();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithDeveloperTools()
                .WithInterFont()
                .LogToTrace();
    }
}