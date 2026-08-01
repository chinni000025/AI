namespace AIEngineUnitTest.Controller
{
    using AIEngineConnectivity.Services;
    using AIEngineGateway.Controllers;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
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
        public async Task ConnectionController_SaveGoogleConnection_WithValidDetails()
        {
            SetUpMockHttpContext(scheme: "https", host: "api.aiengine.com");

            string clientId = "client-id-123";
            string clientSecret = "client-secret-xyz";
            var cancellationToken = CancellationToken.None;
            var result = await _Sut.SaveGoogleConnection(clientId, clientSecret, cancellationToken);
            result.Should().BeOfType<OkResult>();
        }

        private void SetUpMockHttpContext(string scheme = "https", string host = "localhost:5001")
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = scheme;
            httpContext.Request.Host = new HostString(host);

            _Sut.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}
