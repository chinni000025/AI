namespace AIEngineGateway.EngineInfrastructure
{
    using AIEngineGateway.Extensions;
    using Microsoft.Data.SqlClient;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using static AIEngineConnectivity.Constants.EngineConstants;
#nullable disable
    public class EngineConfig
    {
        // Engine Config File (present in programData).
        public const string _folderPath = @"C:\ProgramData\AIEngine";
        public const string _filePath = @"C:\ProgramData\AIEngine\AIEngineConfig.json";
        public bool IsEngineConfig()
        {
            return File.Exists(_filePath);
        }

        public DataBaseProvider GetDatabaseType()
        {
            if (!IsEngineConfig())
                throw new Exception("Engine is not configured");

            string json = File.ReadAllText(_filePath);
            using JsonDocument jsonDocument = JsonDocument.Parse(json);
            int dataBaseType = jsonDocument.RootElement.TryGetProperty("DataBaseType", out JsonElement element)
                ? element.GetInt32() : 1; // default sql because we are previously use sql.
            return (DataBaseProvider)dataBaseType;
        }

        public void SaveEncryptedConnectionString(string connectionString, DataBaseProvider dataBaseProvider)
        {
            Directory.CreateDirectory(_folderPath);
            var encryptedConnectionString = Encryption(connectionString);
            var FileContent = new
            {
                EngineConfigured = true,
                EngineId = encryptedConnectionString,
                DataBaseType = dataBaseProvider.Equals(DataBaseProvider.SqlServer) ? 1 : 2,
                CreatedAt = DateTime.UtcNow
            };
            File.WriteAllText(_filePath, JsonSerializer.Serialize(FileContent));
        }

        public static string Encryption(string connectionString)
        {
            var bytes = Encoding.UTF8.GetBytes(connectionString);
            var protectionData = ProtectedData.Protect(bytes, null, DataProtectionScope.LocalMachine);
            return Convert.ToBase64String(protectionData);
        }

        public static string Decryption(string connectionString)
        {
            var base64String = Convert.FromBase64String(connectionString);
            var unprotectedData = ProtectedData.Unprotect(base64String, null, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(unprotectedData);
        }

        public string ConnectionString()
        {
            if (!File.Exists(_filePath))
                throw new Exception("Engine is Not Configured!.");
            var fileContent = File.ReadAllText(_filePath);
            var document = JsonDocument.Parse(fileContent);
            var encryptedData = document.RootElement.GetProperty("EngineId").GetString();
            return Decryption(encryptedData);
        }

        public async Task<bool> IsDataBaseExist(DataBaseProvider dataBaseProvider, string connnectionString)
        {
            try
            {
                await dataBaseProvider.UseRequiredServer(connnectionString);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
