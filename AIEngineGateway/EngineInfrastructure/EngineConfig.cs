using AIEngineGateway.Extensions;
using Microsoft.AspNetCore.DataProtection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using static AIEngineConnectivity.Constants.EngineConstants;

namespace AIEngineGateway.EngineInfrastructure
{
#nullable disable
    public class EngineConfig
    {
        // Engine Config File (present in program Data).
        public bool IsEngineConfig()
        {
            return File.Exists(GetFilePath());
        }

        public static string GetFolderPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AIEngine");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "/var/lib/AIEngine";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "/Library/Application Support/AIEngine";
            }

            throw new Exception("Platform Not Supported");
        }

        public static string GetFilePath()
        {
            return Path.Combine(GetFolderPath(), "AIEngineConfig.json");
        }

        public DataBaseProvider GetDatabaseType()
        {
            if (!IsEngineConfig())
                throw new Exception("Engine is not configured");

            string json = File.ReadAllText(GetFilePath());
            using JsonDocument jsonDocument = JsonDocument.Parse(json);
            int dataBaseType = jsonDocument.RootElement.TryGetProperty("DataBaseType", out JsonElement element)
                ? element.GetInt32() : 1; // default sql because we are previously use sql.
            return (DataBaseProvider)dataBaseType;
        }

        public void SaveEncryptedConnectionString(string connectionString, DataBaseProvider dataBaseProvider)
        {
            Directory.CreateDirectory(GetFolderPath());
            var encryptedConnectionString = Encryption(connectionString);
            var FileContent = new
            {
                EngineConfigured = true,
                EngineId = encryptedConnectionString,
                DataBaseType = dataBaseProvider.Equals(DataBaseProvider.SqlServer) ? 1 : 2,
                CreatedAt = DateTime.UtcNow
            };
            File.WriteAllText(GetFilePath(), JsonSerializer.Serialize(FileContent));
        }

        public static string Encryption(string connectionString)
        {
            var folderPath = GetFolderPath();
            Directory.CreateDirectory(folderPath);
            var provider = DataProtectionProvider.Create(new DirectoryInfo(folderPath));
            var protector = provider.CreateProtector("AIEngine.ConnectionStrings");
            var bytes = Encoding.UTF8.GetBytes(connectionString);
            var protectedData = protector.Protect(bytes);
            return Convert.ToBase64String(protectedData);
        }

        public static string Decryption(string connectionString)
        {
            var folderPath = GetFolderPath();
            var provider = DataProtectionProvider.Create(new DirectoryInfo(folderPath));
            var protector = provider.CreateProtector("AIEngine.ConnectionStrings");
            var base64Bytes = Convert.FromBase64String(connectionString);
            var unprotectedData = protector.Unprotect(base64Bytes);
            return Encoding.UTF8.GetString(unprotectedData);
        }

        public string ConnectionString()
        {
            if (!File.Exists(GetFilePath()))
                throw new Exception("Engine is Not Configured!.");
            var fileContent = File.ReadAllText(GetFilePath());
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
