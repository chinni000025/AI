
namespace AIEngineConnectivity.Services
{
    using AIEngineConnectivity.Entities;
    public interface IPasswordService
    {
        bool VerifyPassword(User user, string hashedPassword, string inputPassword);
        string HashPassword(User user, string password);
    }
}
