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

        [HttpGet("getTrashFiles")]
        public async Task<ActionResult<List<EngineFileResponse>>> GetTrashFiles(CancellationToken cancellationToken)
        {
            var files = await _engineDriveService.GetTrashFilesAsync(cancellationToken);
            return Ok(files);
        }

        [HttpGet("file/{fileId:guid}/download")]
        public async Task<IActionResult> DownloadFile(Guid fileId, CancellationToken cancellationToken)
        {
            try
            {
                var metadata = await _engineDriveService.GetDownloadMetadataAsync(fileId, cancellationToken);
                if (metadata is null)
                    return NotFound();

                long fileSize = metadata.FileSize;
                long offset = 0;
                long bytesToSend = fileSize;
                bool partial = false;
                var rangeHeader = Request.Headers.Range.ToString();

                if (!string.IsNullOrWhiteSpace(rangeHeader))
                {
                    if (!System.Net.Http.Headers.RangeHeaderValue.TryParse(rangeHeader, out var range) ||
                        range is null || !string.Equals(range.Unit, "bytes", StringComparison.OrdinalIgnoreCase) ||
                        range.Ranges.Count != 1)
                    {
                        return RangeNotSatisfiable(fileSize);
                    }

                    var item = range.Ranges.First();

                    if (fileSize == 0)
                        return RangeNotSatisfiable(fileSize);

                    if (item.From.HasValue)
                    {
                        offset = item.From.Value;
                        if (offset >= fileSize)
                            return RangeNotSatisfiable(fileSize);

                        long end = item.To ?? (fileSize - 1);
                        if (end < offset)
                            return RangeNotSatisfiable(fileSize);

                        end = Math.Min(end, fileSize - 1);
                        bytesToSend = end - offset + 1;
                    }
                    else if (item.To.HasValue && item.To.Value > 0)
                    {
                        bytesToSend = Math.Min(item.To.Value, fileSize);
                        offset = fileSize - bytesToSend;
                    }
                    else
                        return RangeNotSatisfiable(fileSize);
                    partial = true;
                }

                var result = await _engineDriveService.GetDownloadStreamAsync(fileId, offset, bytesToSend, metadata, cancellationToken);

                if (result is null)
                    return NotFound();

                await using (result)
                {
                    Response.StatusCode = partial ? StatusCodes.Status206PartialContent : StatusCodes.Status200OK;

                    Response.ContentType = string.IsNullOrWhiteSpace(result.ContentType) ? "application/octet-stream"
                                            : result.ContentType;

                    Response.ContentLength = bytesToSend;
                    Response.Headers.AcceptRanges = "bytes";

                    Response.Headers.ContentDisposition = new System.Net.Mime.ContentDisposition
                    {
                        FileName = result.FileName,
                        Inline = false // Inline = false tells Chrome: "Do NOT display in tab; download immediately as attachment!"
                    }.ToString();

                    if (partial)
                        Response.Headers.ContentRange = $"bytes {offset}-{offset + bytesToSend - 1}/{fileSize}";


                    var buffer = new byte[81920];
                    long remaining = bytesToSend;

                    while (remaining > 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        int requested = (int)Math.Min(buffer.Length, remaining);
                        int read = await result.Stream.ReadAsync(buffer.AsMemory(0, requested), cancellationToken);
                        if (read == 0)
                            throw new EndOfStreamException("File content ended before the expected length.");

                        await Response.Body.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                        remaining -= read;
                    }
                }

                return new EmptyResult();
            }
            catch
            {
                return BadRequest("Error Occurred While Downloading the File");
            }
        }

        private IActionResult RangeNotSatisfiable(long fileSize)
        {
            Response.Headers.ContentRange = $"bytes */{fileSize}";
            return StatusCode(StatusCodes.Status416RangeNotSatisfiable);
        }
    }
}
