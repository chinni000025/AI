namespace AIEngineInstaller.Services
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.Services;
    using AIEngineCore.EngineCore;
    using AIEngineInstaller.Models;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class InstallerService : IInstallerService
    {
        private readonly IServiceProvider _serviceProvider;
        public InstallerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public ISystemCheckService getRequiredContext()
        {
            var context = _serviceProvider.GetRequiredService<RunningEnvironment>();
            return context.CurrentRunningEnvironment switch
            {
                Platform.Linux
                    => _serviceProvider.GetRequiredService<LinuxSystemCheck>(),
                Platform.Window
                    => _serviceProvider.GetRequiredService<WindowsSystemCheck>(),
                _ => throw new PlatformNotSupportedException()
            };
        }
    }
}
