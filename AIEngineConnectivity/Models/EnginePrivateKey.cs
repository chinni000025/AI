using System.Security.Cryptography;

namespace AIEngineConnectivity.Models
{
    public class EnginePrivateKey
    {
        private RSA _RsaInstance;
        public void SetRSAInstance(string privateKey)
        {
            _RsaInstance?.Dispose();
            _RsaInstance = RSA.Create();
            _RsaInstance.ImportFromPem(privateKey);
        }
        public RSA RSAInstance
        {
            get
            {
                return _RsaInstance;
            }
        }
    }
}
