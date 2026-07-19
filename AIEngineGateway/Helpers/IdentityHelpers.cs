namespace AIEngineGateway.Helpers
{
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Helpers;
    using AIEngineConnectivity.Models;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;

    public class IdentityHelpers : IIdentityHelper
    {
        private readonly JWTConfiguration _jWTConfiguration;
        private readonly ILogger<IdentityHelpers> _logger;
        public IdentityHelpers(IOptions<JWTConfiguration> options,
            ILogger<IdentityHelpers> logger)
        {
            _jWTConfiguration = options.Value;
            _logger = logger;
        }
        public async Task<string> GenerateAccessToken(User user)
        {
            string secretKey = _jWTConfiguration.Key;
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogError("Secret Key Not Found");
                throw new Exception("Secret Key Not Found");
            }

            var encodingKey = new SymmetricSecurityKey(Convert.FromBase64String(_jWTConfiguration.Key));
            var credential = new SigningCredentials(encodingKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {   new Claim(ClaimTypes.Name,user.UserName),
                new Claim(JwtRegisteredClaimNames.Sub,user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken
            (
                issuer: _jWTConfiguration.Issuer,
                audience: _jWTConfiguration.Audience,
                claims: claims,
                signingCredentials: credential,
                expires: DateTime.UtcNow.AddMinutes(_jWTConfiguration.AccessTokenMinutes)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshToken()
        {
            var randomNumber = RandomNumberGenerator.Create();
            var bytes = new byte[64];
            randomNumber.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public string HashRefreshToken(string refreshToken, string secretKey)
        {
            if (refreshToken == null)
            {
                _logger.LogError("Refresh Token can't be Null");
                throw new Exception("Refresh Token can't be Null");
            }

            using var hmacAlgo = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var refreshTokenBytes = Encoding.UTF8.GetBytes(refreshToken);
            var refreshTokenTokenHash = hmacAlgo.ComputeHash(refreshTokenBytes);
            return Convert.ToBase64String(refreshTokenTokenHash);
        }
    }
}
