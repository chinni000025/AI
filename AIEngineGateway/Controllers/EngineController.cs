namespace AIEngineGateway.Controllers
{
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Services;
    using AIEngineGateway.EngineInfrastructure;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class EngineController : ControllerBase
    {
        private readonly EngineConfig _engineConfig;
        private readonly ILogger<EngineController> _logger;
        private readonly IEngineDataBaseService _engineDataBaseService;
        public EngineController(EngineConfig engineConfig, IEngineDataBaseService engineDataBaseService,
            ILogger<EngineController> logger)
        {
            _engineDataBaseService = engineDataBaseService;
            _engineConfig = engineConfig;
            _logger = logger;
        }

        [HttpGet("engine-status")]
        public async Task<ActionResult<SystemStatusResponse>> GetStatusOfEngine()
        {

            bool isEngineConfig = _engineConfig.IsEngineConfig();
            bool isDataBaseExists = false;
            if (isEngineConfig)
            {
                var connectionString = _engineConfig.ConnectionString();
                isDataBaseExists = await _engineConfig.IsDataBaseExist(_engineConfig.GetDatabaseType(), connectionString);
            }
            return Ok(new SystemStatusResponse
            {
                IsDataBaseConfigure = isEngineConfig && isDataBaseExists
            });
        }

        [HttpPost("configure-database")]
        public async Task<ActionResult> ConfigureDataBase(DataBaseConfiguration dataBaseConfiguration)
        {
            try
            {
                await _engineDataBaseService.ConfigureDataBase(dataBaseConfiguration);
                return Ok(new { Message = "Database Configuration Successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("test-engine")]
        public async Task<ActionResult> TestConnectionAsync(DataBaseConfiguration dataBaseConfiguration)
        {
            try
            {
                await _engineDataBaseService.TestConnectionAsync(dataBaseConfiguration);
                return Ok(new { Message = "Connection successful" });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest("Failed to Connect");
            }
        }
    }
}
