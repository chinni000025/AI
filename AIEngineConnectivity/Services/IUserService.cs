using AIEngineConnectivity.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEngineConnectivity.Services
{
    public interface IUserService
    {
        public CurrentUser? GetCurrentUser { get; }
    }
}
