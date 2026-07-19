namespace AIEngineConnectivity.Services
{
    using AIEngineConnectivity.Models;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IUserService
    {
        public CurrentUser? GetCurrentUser { get; }
    }
}
