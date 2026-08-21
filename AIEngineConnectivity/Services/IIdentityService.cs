using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Entities;

namespace AIEngineConnectivity.Services
{
    public interface IIdentityService
    {
        public Task<Object> AllowEngineAccess(User user, CancellationToken cancellationToken);

        public Task<Object> CreateUser(UserRegister userRegister, CancellationToken cancellationToken);

        public Task<Object> Login(UserLogin userLogin, CancellationToken cancellationToken);

        public Task Logout(CancellationToken cancellationToken);

        public Task<object> RefreshToken(CancellationToken cancellationToken);

        public Task<RefreshToken?> RefreshTokenRereviewFromClient(string refreshToken, CancellationToken cancellationToken);

        public Task ForgetPassword(ForgetPasswordRequest forgetPasswordRequest, string scheme, string host, CancellationToken cancellationToken);

        public Task<bool> ResetPassword(ResetPasswordRequest resetPasswordRequest, CancellationToken cancellationToken);
    }
}
