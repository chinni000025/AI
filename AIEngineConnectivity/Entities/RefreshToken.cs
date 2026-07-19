namespace AIEngineConnectivity.Entities
{
    /// <summary>
    /// This Model is used to store the Refresh Token in the Database.
    /// </summary>

    public class RefreshToken
    {
        public int Id { get; set; }

        public string RefreshTokenHash { get; set; }

        public bool? IsRevoked { get; set; }

        public string userId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ExpiresDate { get; set; }
    }
}
