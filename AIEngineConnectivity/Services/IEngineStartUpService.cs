using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Services
{
    public interface IEngineStartUpService
    {
        public Task InitializeAsync();
    }
}
