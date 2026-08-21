using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Models;
using AIEngineConnectivity.Services;

namespace AIEngineGateway.Services
{
    public class UserService : IUserService
    {
        private IHttpContextAccessor _httpContextAccessor;
        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public CurrentUser? GetCurrentUser =>
            _httpContextAccessor?.HttpContext?.Items["CurrentUser"] as CurrentUser;

    }
}
