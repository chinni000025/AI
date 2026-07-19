namespace AIEngineConnectivity.Services
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);

        string Decrypt(string encryptedText);

        Task<string> GetPublicKey(CancellationToken cancellationToken);
    }
}
