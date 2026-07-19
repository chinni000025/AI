namespace AIEngineConnectivity.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IEngineStartUpService
    {
        public Task InitializeAsync();
    }
}
