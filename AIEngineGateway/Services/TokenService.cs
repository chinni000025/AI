namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Helpers;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Services;
    using Microsoft.Extensions.Options;

    public class TokenService : ITokenService
    {
        private readonly IIdentityHelper _IdentityHelper;
        private readonly JWTConfiguration _jWTConfiguration;
        public TokenService(IIdentityHelper identityHelper,
            IOptions<JWTConfiguration> options)
        {
            _IdentityHelper = identityHelper;
            _jWTConfiguration = options.Value;
        }

        public async Task<string> GenerateAccessToken(User user)
        {
            return await _IdentityHelper.GenerateAccessToken(user);
        }

        public async Task<string> GenerateRefreshToken()
        {
            return await _IdentityHelper.GenerateRefreshToken();
        }

        public string TokenHash(string refreshToken)
        {
            return _IdentityHelper.HashRefreshToken(refreshToken,
                _jWTConfiguration.Key);
        }
    }
}
