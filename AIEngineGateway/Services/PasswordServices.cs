
namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Services;
    using Microsoft.AspNetCore.Identity;

    public class PasswordServices : IPasswordService
    {
        private readonly PasswordHasher<User> _PasswordHasher;
        public PasswordServices()
        {
            _PasswordHasher = new PasswordHasher<User>();
        }
        public string HashPassword(User user, string password)
        {
            return _PasswordHasher.HashPassword(user, password);
        }

        public bool VerifyPassword(User user, string hashedPassword, string inputPassword)
        {
            var passwordVerificationResult = _PasswordHasher.VerifyHashedPassword(user, hashedPassword, inputPassword);
            return passwordVerificationResult != PasswordVerificationResult.Failed;
        }
    }
}
