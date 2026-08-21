using AIEngineConnectivity.Entities;

namespace AIEngineConnectivity.Services
{
    public interface IPasswordService
    {
        bool VerifyPassword(User user, string hashedPassword, string inputPassword);
        string HashPassword(User user, string password);
    }
}
