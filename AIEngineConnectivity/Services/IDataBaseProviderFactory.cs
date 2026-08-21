using AIEngineConnectivity.Constants;
using System;
using System.Collections.Generic;
using System.Text;
using static AIEngineConnectivity.Constants.EngineConstants;

namespace AIEngineConnectivity.Services
{
    public interface IDataBaseProviderFactory
    {
        public Func<IDataBaseProvider> GetDataBaseProvider(DataBaseProvider dataBaseProvider);
    }
}
