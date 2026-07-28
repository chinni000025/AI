namespace AIEngineGateway.Repositories
{
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Repositories;
    using AIEngineGateway.EngineInfrastructure;
    using Microsoft.EntityFrameworkCore;

    public class IdentityRepository : IIdentityRepository
    {
        private readonly EngineContext _EngineContext;
        public IdentityRepository(EngineContext engineContext)
        {
            _EngineContext = engineContext;
        }
        public async Task<User?> GetUserByName(string userName, CancellationToken cancellationToken)
        {
            return await _EngineContext.Users.Where(u => u.UserName == userName).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<RefreshToken?> GetRefreshToken(string refreshTokenHash, CancellationToken cancellationToken)
        {
            return await _EngineContext.RefreshTokens.Where(r => r.RefreshTokenHash == refreshTokenHash)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public void RemoveRefreshToken(RefreshToken refreshToken)
        {
            _EngineContext.RefreshTokens.Remove(refreshToken);
        }

        public async Task AddRefreshToken(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            await _EngineContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        }

        public async Task RevokeToken(RefreshToken refreshToken)
        {
            refreshToken.IsRevoked = true;
        }

        public async Task<User?> GetUserById(string userId, CancellationToken cancellationToken)
        {
            return await _EngineContext.Users
                .Where(u => u.Id.ToString() == userId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ResetPasswordToken?> ResetPasswordTokenExistsOrNot(int userId, CancellationToken cancellationToken)
        {
            return await _EngineContext.ResetPasswordTokens.Where(t => t.UserId == userId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<User?> GetUserByEmail(string email, CancellationToken cancellationToken)
        {
            return await _EngineContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync(cancellationToken);
        }

        public void RemoveResetPasswordToken(ResetPasswordToken resetPasswordToken)
        {
            _EngineContext.ResetPasswordTokens.Remove(resetPasswordToken);
        }

        public async Task AddResetPasswordToken(ResetPasswordToken resetPasswordToken, CancellationToken cancellationToken)
        {
            await _EngineContext.ResetPasswordTokens.AddAsync(resetPasswordToken, cancellationToken);
        }
        public async Task<ResetPasswordToken?> GetResetPasswordToken(string token, CancellationToken cancellationToken)
        {
            return await _EngineContext.ResetPasswordTokens.Where(t => t.Token == token)
                                                             .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<String?> GetUserEmailById(String userId, CancellationToken cancellationToken = default)
        {
            return await _EngineContext.Users.Where(u => u.Id == int.Parse(userId)).Select(u => u.Email).FirstOrDefaultAsync();
        }
    }
}
