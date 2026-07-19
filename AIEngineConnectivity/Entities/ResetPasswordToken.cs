namespace AIEngineConnectivity.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// This model is used to Store the Reset Password Token 
    /// Token generated after user initiate the Forget password Request.
    /// </summary>
    public class ResetPasswordToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public bool IsUsed { get; set; }
        public DateTime ExpiresDate { get; set; }
    }
}
