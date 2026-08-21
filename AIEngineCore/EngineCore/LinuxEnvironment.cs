using AIEngineConnectivity.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineCore.EngineCore
{
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

        public bool isDockerAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
