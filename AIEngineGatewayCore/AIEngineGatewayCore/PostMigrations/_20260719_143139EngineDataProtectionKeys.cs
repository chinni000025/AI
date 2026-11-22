using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Entities;
using AIEngineConnectivity.Models;
using AIEngineGateway.Contracts;
using AIEngineGateway.EngineInfrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.Json;

namespace AIEngineGateway.PostMigrations
{
#nullable disable
    public class _20260719_143139EngineDataProtectionKeys : IPostMigration
    {
        public string MigrationName => "_20260719_143139EngineDataProtectionKeys";
        private string _publicKey;
        private string _privateKey;
        public async Task ExecuteAsync(EngineContext engineContext)
        {
            var existing = await engineContext.DataProtectionKeys.FirstOrDefaultAsync(k => k.ProtectionType ==
                                            EngineEncyrption.RSAEncryption);
            if (existing is not null)
                return;

            using (var rsa = RSA.Create(3072))
            {
                _publicKey = rsa.ExportSubjectPublicKeyInfoPem();
                _privateKey = rsa.ExportRSAPrivateKeyPem();
            }
            DataProtectionKey dataProtectionKey = new DataProtectionKey
            {
                ProtectionType = EngineEncyrption.RSAEncryption,
                ProtectionData = JsonSerializer.Serialize(new RSAConfiguration { PrivateKey = _privateKey, PublicKey = _publicKey })
            };
            await engineContext.DataProtectionKeys.AddAsync(dataProtectionKey);
        }
    }
}
