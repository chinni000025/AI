namespace AIEngineConnectivity.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public class CurrentUser
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public bool isAuthenticated { get; set; } = false;

    }
}
