using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Pathoschild.Http.Client;

using TokenProxy.API.Interfaces;
using TokenProxy.API.Options;

namespace TokenProxy.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IOptions<oAuth2Options> _options;
        private readonly IClient _client;

        public TokenService(IOptions<oAuth2Options> options, HttpClient client)
        {
            _options = options;
            _client = new FluentClient(null, client);
        }

        public async Task<string> GetToken(string clientId, string clientSecret)
        {
            var tokenResponse = await _client.PostAsync(_options.Value.TokenEndpoint)
                        .WithBody(p => p.FormUrlEncoded(new
                        {
                            grant_type = "client_credentials",
                            client_id = clientId,
                            client_secret = clientSecret,
                            scope = _options.Value.Scope
                        }))
                        .AsRawJsonObject();
            return tokenResponse["access_token"]?.ToString();
        }
    }
}
