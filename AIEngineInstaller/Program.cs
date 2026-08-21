using AIEngineInstaller.Extensions;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AIEngineInstaller
{
    internal sealed class Program
    {
        public static IServiceProvider provider { get; private set; } = default!;

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
        {
            var builder = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
#if DEBUG
            builder = builder.WithDeveloperTools();
#endif
            return builder;
        }
    }
}