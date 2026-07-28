namespace AIEngineGateway.Services
{
    using AIEngineConnectivity.Constants;
    using AIEngineConnectivity.DTOs;
    using AIEngineConnectivity.Entities;
    using AIEngineConnectivity.Models;
    using AIEngineConnectivity.Repositories;
    using AIEngineConnectivity.Services;
    using System.Text.Json;
#nullable disable
    public class EngineConnectionService : IEngineConnectionService
    {
        private readonly IEngineLatch _EngineLatch;
        private readonly IRepositoryWrapper _Repository;
        private readonly HttpClient _httpClient;
        private readonly IUserService _UserService;
        public EngineConnectionService(IRepositoryWrapper repository, IUserService userService, IEngineLatch engineLatch)
        {
            _Repository = repository;
            _httpClient = new HttpClient();
            _UserService = userService;
            _EngineLatch = engineLatch;
        }

        public async Task GoogleConnectionAuthorizationCode(string authorizationCode, string userId, CancellationToken cancellationToken)
        {
            var connection = await _Repository.ConnectionRepository.GetConnectionsByUserId(userId, Connection.Google, cancellationToken);
            if (connection is not null)
            {
                var existingConnection = _EngineLatch.Deserialize<GoogleTokenResponse>(connection.ConnectionInfo);
                existingConnection.AuthorizationCode = authorizationCode;
                connection.ConnectionInfo = _EngineLatch.Serialize(existingConnection);
                connection.ModifiedAt = DateTime.UtcNow;
            }
            else
            {
                var connectionInfo = new GoogleTokenResponse
                {
                    AuthorizationCode = authorizationCode
                };

                EngineConnection engineConnection = new EngineConnection
                {
                    UserId = int.Parse(userId),
                    ConnectionName = Connection.Google,
                    ConnectionInfo = _EngineLatch.Serialize(connectionInfo),
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                await _Repository.GetEngineRepo<EngineConnection>().AddAsync(engineConnection, cancellationToken);
            }
            await _Repository.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveAndConnectGoogleConnection(string clientId, string clientSecret, string scheme, string host,
                                                    CancellationToken cancellationToken)
        {
            var currentuser = _UserService.GetCurrentUser;
            var connection = await _Repository.ConnectionRepository
                                .GetConnectionsByUserId(currentuser.UserId, Connection.Google, cancellationToken);
            if (connection is null)
                throw new Exception("Can't Connect with Google");
            if (connection is not null && connection.ModifiedAt.AddMinutes(10) < DateTime.UtcNow)
                throw new Exception("Authorization Code Expries");

            var existingConnection = _EngineLatch.Deserialize<GoogleTokenResponse>(connection.ConnectionInfo);
            if (existingConnection.AuthorizationCode == null)
                throw new Exception("No Authorization Code Found!");

            var formData = new Dictionary<string, string>
            {
                [GoogleConnectionConstants.ClientId] = clientId,
                [GoogleConnectionConstants.ClientSecret] = clientSecret,
                [GoogleConnectionConstants.AuthCode] = existingConnection.AuthorizationCode,
                [GoogleConnectionConstants.GrantType] = "authorization_code",
                [GoogleConnectionConstants.RedirectUri] = $"{scheme}://{host}/api/Connection/oauth/google/callback"
            };

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(GoogleConnectionConstants.OAuthTokenEndPoint, content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(json);

            connection.ConnectionInfo = json;
            connection.ModifiedAt = DateTime.UtcNow;
            connection.IsConnected = true;
            await _Repository.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveSmtpConfiguration(SmtpConfiguration smtpConfiguration, CancellationToken cancellationToken)
        {
            var currentUser = _UserService.GetCurrentUser;
            var connection = await _Repository.ConnectionRepository.GetConnectionsByUserId(currentUser.UserId, Connection.Smtp, cancellationToken);
            var serializeConfiguration = _EngineLatch.Serialize(smtpConfiguration);
            if (connection is not null)
            {
                connection.ConnectionInfo = serializeConfiguration;
                connection.ModifiedAt = DateTime.UtcNow;
            }
            else
            {
                EngineConnection engineConnection = new EngineConnection
                {
                    UserId = int.Parse(currentUser.UserId),
                    ConnectionName = Connection.Smtp,
                    ConnectionInfo = serializeConfiguration,
                    IsConnected = true,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };
                await _Repository.GetEngineRepo<EngineConnection>().AddAsync(engineConnection, cancellationToken);
            }
            await _Repository.SaveChangesAsync(cancellationToken);
        }
    }
}
