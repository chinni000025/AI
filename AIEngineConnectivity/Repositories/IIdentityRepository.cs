
namespace AIEngineConnectivity.Repositories
{
    using AIEngineConnectivity.Entities;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IIdentityRepository
    {
        public Task<User?> GetUserByName(string userName, CancellationToken cancellationToken);

        public Task<RefreshToken?> GetRefreshToken(string refreshTokenHash, CancellationToken cancellationToken);

        public void RemoveRefreshToken(RefreshToken refreshToken);

        public Task AddRefreshToken(RefreshToken refreshToken, CancellationToken cancellationToken);

        public Task RevokeToken(RefreshToken refreshToken);

        public Task<User?> GetUserById(string userId, CancellationToken cancellationToken);

        public Task AddNewUser(User user, CancellationToken cancellationToken);

        public Task<ResetPasswordToken?> ResetPasswordTokenExistsOrNot(int userId, CancellationToken cancellationToken);

        public Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken);

        public void RemoveResetPasswordToken(ResetPasswordToken resetPasswordToken);

        public Task AddResetPasswordToken(ResetPasswordToken resetPasswordToken, CancellationToken cancellationToken);

        public Task<ResetPasswordToken?> GetResetPasswordToken(string token, CancellationToken cancellationToken);
        public Task<String?> GetUserEmailById(String userId, CancellationToken cancellationToken = default);

    }
}
