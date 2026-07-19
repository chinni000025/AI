namespace AIEngineConnectivity.Services
{
    using AIEngineConnectivity.Constants;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using static AIEngineConnectivity.Constants.EngineConstants;
    public interface IDataBaseProviderFactory
    {
        public Func<IDataBaseProvider> GetDataBaseProvider(DataBaseProvider dataBaseProvider);
    }
}
