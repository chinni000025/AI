namespace AIEngineCore.EngineCore
{
    using AIEngineConnectivity.Services;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public class LinuxEnvironment : IEnvironment
    {
        public bool CanInstallEngine()
        {
            throw new NotImplementedException();
        }

        public Task installDocker()
        {
            throw new NotImplementedException();
        }
    }
}
