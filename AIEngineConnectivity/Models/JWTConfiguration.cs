namespace AIEngineConnectivity.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public class JWTConfiguration
    {
        public string Key { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public int AccessTokenMinutes { get; set; } = 5;

        public int RefreshTokenDays { get; set; } = 2;
    }
}
