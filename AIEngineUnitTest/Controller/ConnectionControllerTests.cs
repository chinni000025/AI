namespace AIEngineUnitTest.Controller
{
    using AIEngineConnectivity.Services;
    using FakeItEasy;
    using Xunit;

    public class ConnectionControllerTests
    {
        private IEngineConnectionService _EngineConnectionService;
        private IEmailService _EmailService;

        public ConnectionControllerTests()
        {
            _EngineConnectionService = A.Fake<IEngineConnectionService>();
            _EmailService = A.Fake<IEmailService>();
        }

        [Fact]
        public void ConnectionController_SaveGoogleConnection_WithValidDetails()
        {

        }
    }
}
