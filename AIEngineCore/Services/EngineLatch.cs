using AIEngineConnectivity.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AIEngineCore.Services
{
#nullable disable
    public class EngineLatch : IEngineLatch
    {
        public string Serialize<T>(T value)
        {
            if (value is not null)
            {
                return JsonSerializer.Serialize(value);
            }
            return string.Empty;
        }
        public T Deserialize<T>(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            return default;
        }
    }
}
