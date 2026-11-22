using AIEngineConnectivity.Constants;
using AIEngineConnectivity.Models;
using AIEngineConnectivity.Repositories;
using AIEngineConnectivity.Services;
using System.Security.Cryptography;
using System.Text;

namespace AIEngineGateway.Services
{
#nullable disable
    public class EncryptionService : IEncryptionService
    {

        private readonly IRepositoryWrapper _Repository;
        private readonly IEngineLatch _EngineLatch;
        private EnginePrivateKey _EngineRSAInstance;

        public EncryptionService(IRepositoryWrapper repositoryWrapper, IEngineLatch engineLatch, EnginePrivateKey enginePrivateKey)
        {
            _Repository = repositoryWrapper;
            _EngineLatch = engineLatch;
            _EngineRSAInstance = enginePrivateKey;
        }

        public async Task<string> GetPublicKey(CancellationToken cancellationToken)
        {
            var dataProtection = await _Repository.DataProtectionKeyRepository.GetKeyAsync(EngineEncyrption.RSAEncryption, cancellationToken);
            if (dataProtection is null)
                return string.Empty;
            var privateAndPublicKey = _EngineLatch.Deserialize<RSAConfiguration>(dataProtection?.ProtectionData);
            _EngineRSAInstance.SetRSAInstance(privateAndPublicKey.PrivateKey);
            return privateAndPublicKey.PublicKey;
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
                return string.Empty;
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] decryptedBytes = _EngineRSAInstance.RSAInstance.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        // For future purpose
        public string Encrypt(string plainText)
        {
            throw new NotImplementedException();
        }
    }
}