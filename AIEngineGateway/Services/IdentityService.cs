namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.EngineCore;
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Helpers;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Repositories;
    using AIEngineConnectivity.Services;
    using AIEngineCore.EngineCore;
    using AIEngineCore.EngineNotifications;
    using AIEngineGateway.Hub;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Options;
    using System.Security.Cryptography;
    using System.Text;

    public class IdentityService : IIdentityService
    {

        private readonly IPasswordService _passwordService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenService _TokenService;
        private readonly JWTConfiguration _jWTConfiguration;
        private readonly IRepositoryWrapper _Repository;
        private readonly IUserSessionManager _userSessionManager;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IAntiforgery _antiforgery;
        private readonly IServiceProvider _ServiceProvider;

        public IdentityService(IPasswordService passwordService,
            IIdentityHelper identityHelper, IHttpContextAccessor httpContextAccessor,
            IOptions<JWTConfiguration> options,
            ITokenService tokenService,
            IRepositoryWrapper repositoryWrapper,
            IIdentityRepository identityRepository,
            IUserSessionManager userSessionManager,
            IAntiforgery antiforgery,
            IHubContext<NotificationHub> hubContext,
            IServiceProvider serviceProvider)
        {
            _passwordService = passwordService;
            _httpContextAccessor = httpContextAccessor;
            _TokenService = tokenService;
            _jWTConfiguration = options.Value;
            _Repository = repositoryWrapper;
            _userSessionManager = userSessionManager;
            _hubContext = hubContext;
            _antiforgery = antiforgery;
            _ServiceProvider = serviceProvider;
        }

        public async Task<object> AllowEngineAccess(User user, CancellationToken cancellationToken)
        {
            var ExistingUser = await _Repository.IdentityRepository.GetUserByName(user.UserName, cancellationToken);
            if (ExistingUser == null)
                throw new Exception("No User Found");

            var accessToken = await _TokenService.GenerateAccessToken(ExistingUser);
            var refreshToken = await _TokenService.GenerateRefreshToken();
            var refreshTokenHash = _TokenService.TokenHash(refreshToken);
            var ExistingRefreshToken = await _Repository.IdentityRepository.GetRefreshToken(refreshTokenHash, cancellationToken);

            if (ExistingRefreshToken != null)
                _Repository.IdentityRepository.RemoveRefreshToken(ExistingRefreshToken);

            var newRefreshToken = new RefreshToken
            {
                userId = ExistingUser.Id.ToString(),
                CreatedDate = DateTime.UtcNow,
                RefreshTokenHash = refreshTokenHash,
                ExpiresDate = DateTime.UtcNow.AddDays(_jWTConfiguration.RefreshTokenDays)
            };

            await _Repository.IdentityRepository.AddRefreshToken(newRefreshToken, cancellationToken);
            await _Repository.SaveChangesAsync(cancellationToken);
            var response = _httpContextAccessor.HttpContext?.Response;

            if (response == null)
                throw new Exception("Internal Server Error");

            var engineAntiforgery = _antiforgery.GetAndStoreTokens(_httpContextAccessor.HttpContext);
            response?.Cookies.Append(AuthConstants.EngineRestart, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = true,
                Path = "/",
                IsEssential = true,
                Expires = DateTime.UtcNow.AddDays(_jWTConfiguration.RefreshTokenDays)
            });

            return new
            {
                EngineIgnition = accessToken,
                EngineValidation = engineAntiforgery.RequestToken
            };
        }

        public async Task<Object> CreateUser(UserRegister userRegister, CancellationToken cancellationToken)
        {
            var user = await _Repository.IdentityRepository.GetUserByName(userRegister.UserName, cancellationToken);
            if (user != null)
                throw new Exception("User Already Exists");
            var userEmail = await _Repository.IdentityRepository.GetUserByEmail(userRegister.Email, cancellationToken);
            if (userEmail != null)
                throw new Exception("User Email is Already Exist");
            User newUser = new User
            {
                UserName = userRegister.UserName,
                Email = userRegister.Email,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };
            var eventPublisher = _ServiceProvider.GetRequiredService<EngineEventPublisher>();
            await eventPublisher.PublishEvent(new EngineNotificationMessage
            {
                EngineEvents = EngineEvents.UserCreated,
                Notification = new EngineEmailNotification
                {
                    ToAddress = newUser.Email,
                    Subject = "Account Created Successfully",
                    parameters = new Dictionary<string, string>
                    {
                        ["LogoUrl"] = "https://your-domain.com/assets/logo.png",
                        ["Name"] = newUser.UserName,
                        ["Email"] = newUser.Email,
                        ["ActionUrl"] = "https://your-domain.com",
                        ["SupportEmail"] = "support@your-domain.com",
                        ["Year"] = DateTime.UtcNow.Year.ToString()
                    }
                }
            });
            var hashedPassword = _passwordService.HashPassword(newUser, userRegister.Password);
            newUser.Password = hashedPassword;
            await _Repository.GetEngineRepo<User>().AddAsync(newUser, cancellationToken);
            await _Repository.SaveChangesAsync(cancellationToken);

            return new
            {
                UserName = newUser.UserName
            };
        }

        public async Task<object> Login(UserLogin userLogin, CancellationToken cancellationToken)
        {
            var user = await _Repository.IdentityRepository.GetUserByName(userLogin.UserName, cancellationToken);
            if (user == null)
                throw new Exception("User Not Exists");
            var CorrectAuthentication = _passwordService.VerifyPassword(user, user.Password, userLogin.Password);
            if (!CorrectAuthentication)
                throw new Exception("Password Incorrect");
            var otherSessions = _userSessionManager
               .GetOtherSessionConnections(user.Id.ToString(), userLogin.SessionId);

            foreach (var conn in otherSessions)
            {
                await _hubContext.Clients.Client(conn)
                    .SendAsync(EngineConstants.ForceLogout, "Logged In other session");
            }
            return await AllowEngineAccess(user, cancellationToken);
        }

        public async Task Logout(CancellationToken cancellationToken)
        {
            var response = _httpContextAccessor?.HttpContext?.Response;
            var request = _httpContextAccessor?.HttpContext?.Request;
            if (request?.Cookies[AuthConstants.EngineRestart] == null || request?.Cookies.Count == 0)
                return;

            var refreshToken = await RefreshTokenRereviewFromClient(request?.Cookies[AuthConstants.EngineRestart], cancellationToken);
            response?.Cookies.Delete(AuthConstants.EngineRestart, new CookieOptions
            {
                SameSite = SameSiteMode.Lax,
                Secure = true,
                Path = "/"
            });

            if (refreshToken == null)
                throw new Exception("Token Not Exist");

            if (refreshToken != null)
            {
                await _Repository.IdentityRepository.RevokeToken(refreshToken);
                await _Repository.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<object> RefreshToken(CancellationToken cancellationToken)
        {
            var response = _httpContextAccessor?.HttpContext?.Response;
            var request = _httpContextAccessor?.HttpContext?.Request;
            if (request?.Cookies[AuthConstants.EngineRestart] == null || request?.Cookies.Count == 0)
                throw new Exception("Needs to Login");
            var refreshToken = await RefreshTokenRereviewFromClient(request?.Cookies[AuthConstants.EngineRestart], cancellationToken);
            response?.Cookies.Delete(AuthConstants.EngineRestart, new CookieOptions
            {
                SameSite = SameSiteMode.Lax,
                Secure = true,
                Path = "/"
            });

            if (refreshToken == null)
                throw new Exception("Token Not Exist");

            if (refreshToken.ExpiresDate < DateTime.UtcNow)
            {
                await _Repository.IdentityRepository.RevokeToken(refreshToken);
                await _Repository.SaveChangesAsync(cancellationToken);
                throw new Exception("Needs to Login");
            }

            if (refreshToken.IsRevoked == true)
            {
                throw new Exception("Needs to Login");
            }
            else
            {
                var user = await _Repository.GetEngineRepo<User>().GetByIdAsync(int.Parse(refreshToken.userId), cancellationToken);
                if (user == null)
                {
                    throw new Exception("User Not Exist");
                }
                return await AllowEngineAccess(user, cancellationToken);
            }
        }

        public async Task<RefreshToken?> RefreshTokenRereviewFromClient(string refreshToken, CancellationToken cancellationToken)
        {
            var refreshTokenHash = _TokenService.TokenHash(refreshToken);
            return await _Repository.IdentityRepository.GetRefreshToken(refreshTokenHash, cancellationToken);
        }

        public async Task ForgetPassword(ForgetPasswordRequest forgetPasswordRequest, string scheme, string host, CancellationToken cancellationToken)
        {
            var user = await _Repository.GetEngineRepo<User>()
                        .FirstOrDefaultAsync(u => u.Email == forgetPasswordRequest.Email, cancellationToken);
            if (user is null)
                return;

            var random = RandomNumberGenerator.Create();
            var bytes = new byte[64];
            random.GetBytes(bytes);
            var token = Convert.ToBase64String(bytes);
            var tokenHash = _TokenService.TokenHash(token);
            var resetPasswordTokenExists = await _Repository.IdentityRepository.ResetPasswordTokenExistsOrNot(user.Id, cancellationToken);

            if (!(resetPasswordTokenExists is null))
                _Repository.IdentityRepository.RemoveResetPasswordToken(resetPasswordTokenExists);

            var resetPasswordToken = new ResetPasswordToken
            {
                UserId = user.Id,
                Token = tokenHash,
                ExpiresDate = DateTime.UtcNow.AddMinutes(5) //Expires in 5 minutes
            };

            await _Repository.IdentityRepository.AddResetPasswordToken(resetPasswordToken, cancellationToken);
            await _Repository.SaveChangesAsync(cancellationToken);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetUrl = $"{scheme}://{host}/reset-identity" +
                                     $"?email={Uri.EscapeDataString(user.Email)}" +
                                     $"&token={encodedToken}";
            string body = $@"
                                Dear User,

                                We received a request to reset your AI Engine account password.

                                Please click the link below to reset your password:
                                
                                {resetUrl}

                                This link is valid for a limited time.
                                If you did not request a password reset, please ignore this email.

                                Best regards,
                                AI Engine";

            try
            {
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> ResetPassword(ResetPasswordRequest resetPasswordRequest, CancellationToken cancellationToken)
        {
            var user = await _Repository.IdentityRepository.GetUserByEmail(resetPasswordRequest.Email, cancellationToken);
            if (user is null)
                return false;
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordRequest.Token));
            if (token == null)
                return false;

            var tokenHash = _TokenService.TokenHash(token);
            var existingToken = await _Repository.IdentityRepository.GetResetPasswordToken(tokenHash, cancellationToken);
            if (existingToken is null || existingToken.ExpiresDate < DateTime.UtcNow)
                return false;

            user.Password = _passwordService.HashPassword(user, resetPasswordRequest.NewPassword);
            existingToken.IsUsed = true;
            await _Repository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
