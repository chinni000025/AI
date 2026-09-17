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
        public async Task<ActionResult> InitiateUpload([FromBody] UploadInitiateRequest request, CancellationToken cancellationToken)
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
        public async Task<ActionResult> FinalizeUpload([FromQuery] Guid sessionId, CancellationToken cancellationToken)
        {
            try
            {
                await _engineDriveService.FinalizeUploadAsync(sessionId, cancellationToken);
                return Ok();
            }
            catch
            {
                return BadRequest("Error Occurred While saving the File");
            }
        }

        [HttpGet("getEngineFiles")]
        public async Task<ActionResult<List<EngineFileResponse>>> GetEngineFiles(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _engineDriveService.GetAvailableFilesAsync(cancellationToken);
                return Ok(result);
            }
            catch
            {
                return BadRequest("Error Occured while Getting Engine Files");
            }
        }

        [HttpGet("getEngineStorageInfo")]
        public async Task<ActionResult<EngineFileStorageInfo>> GetStorageInfor(CancellationToken cancellationToken)
        {
            try
            {
                var info = await _engineDriveService.GetEngineStorageInfo(cancellationToken);
                return Ok(info);
            }
            catch
            {
                return BadRequest("Error Ocurred While Getting Storage Info");
            }
        }

        [HttpPost("deleteEngineFiles")]
        public async Task<IActionResult> DeleteEngineFiles([FromBody] List<Guid> FileIds, CancellationToken cancellationToken)
        {
            try
            {
                await _engineDriveService.DeleteFileByIds(FileIds, cancellationToken);
                return Ok();
            }
            catch
            {
                return BadRequest("Can't able to Delete the Files");
            }
        }
    }
}
