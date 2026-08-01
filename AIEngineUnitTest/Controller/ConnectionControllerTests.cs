namespace AIEngineUnitTest.Controller
{
    using AIEngineConnectivity.Services;
    using AIEngineGateway.Controllers;
    using FakeItEasy;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class ConnectionControllerTests
    {
        private readonly IEngineConnectionService _EngineConnectionService;
        private readonly IEmailService _EmailService;
        private readonly ILogger<ConnectionController> _Logger;
        private readonly ConnectionController _Sut;

        public ConnectionControllerTests()
        {
            _EngineConnectionService = A.Fake<IEngineConnectionService>();
            _EmailService = A.Fake<IEmailService>();
            _Logger = A.Fake<ILogger<ConnectionController>>();
            _Sut = new ConnectionController(_EngineConnectionService, _Logger, _EmailService);
        }

        [Fact]
        public void ConnectionController_SaveGoogleConnection_WithValidDetails()
        {

        }
    }
}
