namespace AIEngineConnectivity.Helpers
{
    using AIEngineConnectivity.Entities;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public interface IIdentityHelper
    {
        public Task<string> GenerateAccessToken(User user);

        public Task<string> GenerateRefreshToken();

        public string HashRefreshToken(string refreshToken, string secretKey);
    }
}
