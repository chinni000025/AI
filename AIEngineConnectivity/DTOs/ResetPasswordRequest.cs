
namespace AIEngineConnectivity.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Text;
#nullable disable
    public class ResetPasswordRequest
    {
        public string Email { get; set; }

        public string Token { get; set; }

        public string NewPassword { get; set; }
    }
}
