using AIEngineConnectivity.Constants;
using AIEngineConnectivity.DTOs;
using AIEngineConnectivity.Models;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;
using System;

namespace AIEngineCore.Providers
{
    public class GeminiProvider : ProviderBase.ProviderBase
    {
        private readonly Client _client;
        private readonly GeminiAPiKeyConfiguration _geminiAPiKeyConfiguration;
        public GeminiProvider(IServiceProvider serviceProvider, IOptions<GeminiAPiKeyConfiguration> options)
        {
            _geminiAPiKeyConfiguration = options.Value;
            if (string.IsNullOrWhiteSpace(_geminiAPiKeyConfiguration.ApiKey))
                throw new Exception("Gemini API Key is NOT configured!.");
            _client = new Client(apiKey: _geminiAPiKeyConfiguration.ApiKey);
        }

        public override async Task<AIResponse?> GenerateAsync(AIRequest request)
        {
            // for geminin provider we are not provide the message history due to the limitation of tokens.
            try
            {
                var systemInstruction = new Content
                {
                    Parts = new List<Part>
                    {
                        new Part { Text = GeminiConfigurations.SystemPrompt }
                    }
                };

                var contents = new List<Content>
                {
                    new Content
                    {
                        Role = EngineRoles.User,
                        Parts = new List<Part>
                        {
                            new Part { Text = request.Prompt }
                        }
                    }
                };

                var response = await _client.Models.GenerateContentAsync(
                    model: request.Model,
                    contents: contents,
                    config: new GenerateContentConfig
                    {
                        SystemInstruction = systemInstruction,
                        Temperature = GeminiConfigurations.Temperature,
                        MaxOutputTokens = GeminiConfigurations.MaxTokens
                    }
                );

                var output = response?.Candidates?
                    .FirstOrDefault()?
                    .Content?.Parts?
                    .FirstOrDefault()?
                    .Text;
                return CreateResponse(output);
            }
            catch
            {
                throw;
            }
        }
    }
}
