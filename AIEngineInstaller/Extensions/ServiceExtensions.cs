namespace AIEngineInstaller.Extensions
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.Services;
    using AIEngineCore.EngineCore;
    using AIEngineInstaller.Models;
    using AIEngineInstaller.Services;
    using Microsoft.Extensions.DependencyInjection;
    public static class ServiceExtensions
    {
        public static void addServices(this IServiceCollection services)
        {
            services.AddSingleton<WindowsEnvironment>();
            services.AddSingleton<LinuxEnvironment>();
            services.AddSingleton<IInstallerService, InstallerService>();
            services.AddSingleton<IAIEngineGatewayManagerService, AIEngineGatewayManagerService>();
            services.AddSingleton<IBrowserService, WindowsBrowserService>();
            services.AddSingleton<RunningEnvironment>(serviceProvider =>
            {
                Platform platform;
                if (System.OperatingSystem.IsLinux())
                {
                    platform = Platform.Linux;
                }
                else
                {
                    platform = Platform.Window;
                }
                return new RunningEnvironment
                {
                    CurrentRunningEnvironment = platform
                };
            });
        }
    }
}
