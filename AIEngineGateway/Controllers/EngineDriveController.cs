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
            var uploadSessionId = await _engineDriveService.InitiateFileUpload(request, cancellationToken);
            return Ok(new { uploadSessionId });
        }

        [HttpPost("uploadChunks")]
        public async Task<ActionResult> UploadChunks([FromForm] IFormFile chunk, [FromForm] long chunkIndex, [FromForm] Guid sessionId, CancellationToken cancellationToken)
        {
            if (chunk is null || chunk.Length < 0 || chunkIndex < 0 || sessionId == Guid.Empty)
                return BadRequest();
            await _engineDriveService.UploadChunks(chunk, chunkIndex, sessionId, cancellationToken);
            return Ok();
        }

        [HttpPost("finalize")]
        public async Task<ActionResult> FinalizeUpload([FromBody] Guid sessionId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
