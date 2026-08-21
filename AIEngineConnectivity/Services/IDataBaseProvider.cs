using AIEngineConnectivity.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Services
{
    public interface IDataBaseProvider
    {
        public string BuildConnectionString(DataBaseConfiguration dataBaseConfiguration);
    }
}