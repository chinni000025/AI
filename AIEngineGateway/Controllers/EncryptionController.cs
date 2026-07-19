namespace AIEngineGateway.Controllers
{
    using AIEngineConnectivity.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class EncryptionController : ControllerBase
    {
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<EncryptionController> _logger;

        public EncryptionController(IEncryptionService encryptionService, ILogger<EncryptionController> logger)
        {
            _encryptionService = encryptionService;
            _logger = logger;
        }

        [HttpGet("public-key")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> GetPublicKey(CancellationToken cancellation)
        {
            return Ok(await _encryptionService.GetPublicKey(cancellation));
        }

    }
}
