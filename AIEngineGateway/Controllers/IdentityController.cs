using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIEngineGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<IdentityController> _logger;
        public IdentityController(IIdentityService identityService, ILogger<IdentityController> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        [HttpPost]
        [Route("user-login")]
        public async Task<IActionResult> UserLogin(UserLogin userLogin, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _identityService.Login(userLogin, cancellationToken);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while login to AIEngine" + ex);
                return BadRequest(ex.Message);
            }

        }

        [HttpPost]
        [Route("user-register")]
        public async Task<IActionResult> UserRegister(UserRegister userRegister, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _identityService.CreateUser(userRegister, cancellationToken);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Occured While User Register", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("refresh-token")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetAccessTokenUsingRefreshToken(CancellationToken cancellationToken)
        {
            try
            {
                var response = await _identityService.RefreshToken(cancellationToken);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("user-logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserLogout(CancellationToken cancellationToken)
        {
            try
            {
                await _identityService.Logout(cancellationToken);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("forget-identity")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPasswordRequest, CancellationToken cancellationToken)
        {
            try
            {
                await _identityService.ForgetPassword(forgetPasswordRequest, Request.Scheme, Request.Host.ToString(), cancellationToken);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("reset-identity")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _identityService.ResetPassword(resetPasswordRequest, cancellationToken);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
