namespace AIEngineConnectivity.Services
{
    using AIEngineConnectivity.Entities;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface ITokenService
    {
        public Task<string> GenerateAccessToken(User user);
        public Task<string> GenerateRefreshToken();
        public string TokenHash(string refreshToken);
    }
}
