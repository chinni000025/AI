using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;

namespace AIEngineGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EngineStateController : ControllerBase
    {
        private EngineState _engineState;

        public EngineStateController(EngineState engineState)
        {
            _engineState = engineState;
        }


        [HttpGet("engine-status")]
        [AllowAnonymous]
        [OutputCache(PolicyName = CachePolicies.ShortLivedCachePolicy)]
        public IActionResult GetEngineState()
        {
            return Ok(new
            {
                isEngineRunning = _engineState.IsEngineRunning,
                isEngineReady = _engineState.IsEngineReady,
                errorMessage = _engineState.ErrorMessage
            });
        }
    }
}
