using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Services
{
    public interface IEngineLatch
    {
        public string Serialize<T>(T value);
        public T Deserialize<T>(string value);
    }
}
