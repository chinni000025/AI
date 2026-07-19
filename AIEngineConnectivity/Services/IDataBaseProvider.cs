namespace AIEngineConnectivity.Services
{
    using AIEngineConnectivity.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public interface IDataBaseProvider
    {
        public string BuildConnectionString(DataBaseConfiguration dataBaseConfiguration);
    }
}