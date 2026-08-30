using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIEngineGateway.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EngineDriveController : ControllerBase
    {

        private readonly IEngineDriveService _engineDriveService;
        public EngineDriveController(IEngineDriveService engineDriveService)
        {
            _engineDriveService = engineDriveService;
        }

        [HttpPost("initiate-upload")]
        public async Task<ActionResult> InitateUpload([FromBody] UploadInitiateRequest request, CancellationToken cancellationToken)
        {
            var chunkBytes = await _engineDriveService.InitiateFileUpload(request, cancellationToken);
            return Ok(chunkBytes);
        }
    }
}
