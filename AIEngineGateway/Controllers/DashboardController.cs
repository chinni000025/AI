namespace AIEngineGateway.Controllers
{
    using AIEngineConnectivity.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;
        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("GetModels")]
        public async Task<IActionResult> GetModels(CancellationToken cancellationToken) //used for get all the conversations to display side nav bar in prompt space.
        {
            try
            {
                var models = ModelCatalog.Providers;
                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}
