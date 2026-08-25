using AIEngineConnectivity.Constants;
using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Models;
using AIEngineConnectivity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace AIEngineGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly IWhisperService _whisperService;
        private readonly ILogger<ConversationController> _logger;
        public ConversationController(IConversationService conversationService, IWhisperService whisperService,
            ILogger<ConversationController> logger)
        {
            _conversationService = conversationService;
            _whisperService = whisperService;
            _logger = logger;
        }

        [HttpGet]
        [Route("GetConversations")]
        public async Task<IActionResult> GetConversations(CancellationToken cancellationToken) //used for get all the conversations to display side nav bar in prompt space.
        {
            try
            {
                var conversations = await _conversationService.GetConversations(cancellationToken);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        [Route("GetFavouriteConversations")]
        public async Task<IActionResult> GetFavouriteConversations(CancellationToken cancellationToken) //used for get all the favourite conversations to display side nav bar in prompt space.
        {
            try
            {
                var conversations = await _conversationService.GetFavouriteConversations(cancellationToken);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        [Route("GetConversation/{conversationId}")]
        public async Task<IActionResult> GetConversationById(Guid conversationId, CancellationToken cancellationToken) // used to get the single conversations.
        {
            try
            {
                var conversation = await _conversationService.GetConversationById(conversationId, cancellationToken);
                return Ok(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [Route("send/{conversationId}/message")]
        public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] AIRequest aiRequest,
            CancellationToken cancellationToken)
        {
            try
            {
                MessagePayload messagePayload = new MessagePayload()
                {
                    Content = aiRequest.Prompt,
                    Model = aiRequest.Model,
                    Provider = aiRequest.Provider
                };

                var response = await _conversationService.SendMessage(conversationId, messagePayload, cancellationToken);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [Route("send/message")]
        public async Task<IActionResult> SendNewMessage([FromBody] AIRequest aiRequest, CancellationToken cancellationToken)
        {
            try
            {
                MessagePayload messagePayload = new MessagePayload()
                {
                    Content = aiRequest.Prompt,
                    Model = aiRequest.Model,
                    Provider = aiRequest.Provider
                };

                var response = await _conversationService.SendMessage(null, messagePayload, cancellationToken);
                return Ok(new { response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete/{conversationId}")]
        public async Task<IActionResult> DeleteConversation(Guid conversationId, CancellationToken cancellationToken)
        {
            var response = await _conversationService.DeleteConversation(conversationId, cancellationToken);
            return Ok(response);
        }

        [HttpPatch]
        [Route("Update/{conversationId}")]
        public async Task<IActionResult> UpdateConversation(Guid conversationId, [FromBody]
        JsonPatchDocument<ConversationPathDTO> jsonPatchDocument, CancellationToken cancellationToken)
        {
            try
            {
                if (jsonPatchDocument is null)
                    return BadRequest("Updating Entity should not be null");

                var operation = jsonPatchDocument.Operations.First();
                if (!string.Equals(operation.op, "replace", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Only Updating is Allowed");

                HashSet<string> allowedPaths = new HashSet<string>
            {
                ConversationUpdatingPaths.Title,
                ConversationUpdatingPaths.IsArchived,
                ConversationUpdatingPaths.IsFavorite,
                ConversationUpdatingPaths.IsPinned,
                ConversationUpdatingPaths.ModelUsed
            };
                if (!allowedPaths.Contains(operation.path))
                    return BadRequest($"Operation {operation.path} is not allowed");

                if (operation.path.Equals(ConversationUpdatingPaths.Title, StringComparison.OrdinalIgnoreCase)
                    && (operation.value is not string givenTitle || string.IsNullOrWhiteSpace(givenTitle)))
                    return BadRequest("Invalid Title For The Conversation");

                if (operation.path.Equals(ConversationUpdatingPaths.ModelUsed, StringComparison.OrdinalIgnoreCase)
                    && (operation.value is not string modelUsed || string.IsNullOrWhiteSpace(modelUsed)))
                    return BadRequest("Invalid Model For The Conversation");

                var response = await _conversationService.UpdateConversation(conversationId, jsonPatchDocument,
                                        cancellationToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        [HttpPost("speech/transcribe")]
        public async Task<IActionResult> UploadAudio(IFormFile audioFile, CancellationToken cancellation)
        {
            try
            {
                string tempInputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{audioFile.FileName}");

                using (var stream = new FileStream(tempInputPath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }

                var text = await _whisperService.TranscribeAudioAsync(tempInputPath, cancellation);
                return Ok(new { text = text });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("GetArchiveChats")]
        public async Task<IActionResult> GetArchiveChats([FromQuery] ArchiveChatRequest archieveChatRequest, CancellationToken cancellationToken)
        {
            try
            {
                //default values.
                if (archieveChatRequest.Page <= 0)
                    archieveChatRequest.Page = 1;

                if (archieveChatRequest.PageSize <= 0)
                    archieveChatRequest.PageSize = 10;

                var response = await _conversationService.GetArchiveChatsAsync(archieveChatRequest, cancellationToken);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
            }
        }

        [HttpPost("uploadFile")]
        [RequestSizeLimit(209_715_200)]
        public async Task<IActionResult> UploadFile([FromQuery] Guid ConversationId, [FromForm] IFormFile file,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
