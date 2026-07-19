namespace AIEngineGateway.Controllers
{
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ConnectionController : ControllerBase
    {
        private readonly IEngineConnectionService _EngineConnectionService;
        private readonly ILogger<ConnectionController> _logger;
        private readonly IEmailService _EmailService;
        public ConnectionController(IEngineConnectionService EngineConnectionService, ILogger<ConnectionController> logger, IEmailService emailService)
        {
            _EngineConnectionService = EngineConnectionService;
            _logger = logger;
            _EmailService = emailService;
        }

        [HttpGet("oauth/google/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state, CancellationToken cancellationToken)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return BadRequest("Authorization code is missing.");
                if (string.IsNullOrWhiteSpace(state))
                    return BadRequest("State (UserId) is missing.");

                await _EngineConnectionService.GoogleConnectionAuthorizationCode(code, state, cancellationToken);
                return Content("""
                        <html>
                        <body>
                        <script>
                        window.opener.postMessage(
                        {
                            type:'google-drive-connected'
                        },
                        '*');
                        window.close();
                        </script>
                        </body>
                        </html>
                        """, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        [HttpPost("saveGoogleConnection")]

        public async Task<IActionResult> SaveGoogleConnection([FromQuery] string clientId, string clientSecret, CancellationToken cancellationToken)
        {
            await _EngineConnectionService.SaveAndConnectGoogleConnection(clientId, clientSecret, Request.Scheme,
                                Request.Host.ToString(), cancellationToken);
            return Ok();
        }

        [HttpPost("testMail")]
        public async Task<IActionResult> TestMail([FromBody] SmtpConfiguration smtpConfiguration, CancellationToken cancellationToken)
        {
            await _EmailService.SendTestMail(smtpConfiguration);
            return Ok();
        }

        [HttpPost("savesmtpConfiguration")]
        public async Task<IActionResult> SaveSmtpConfiguration([FromBody] SmtpConfiguration smtpConfiguration, CancellationToken cancellationToken)
        {
            await _EngineConnectionService.SaveSmtpConfiguration(smtpConfiguration, cancellationToken);
            return Ok();
        }
    }
}
