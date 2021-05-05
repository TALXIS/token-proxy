using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Options;

using Pathoschild.Http.Client;

using TokenProxy.API.Interfaces;
using TokenProxy.API.Options;

namespace TokenProxy.API.Functions
{
    public class ProxyEndpoint
    {
        private readonly ITokenService _tokenService;
        private readonly IHttpClientFactory _factory;
        private readonly IOptions<ApiOptions> _options;

        public ProxyEndpoint(ITokenService tokenService, IHttpClientFactory factory, IOptions<ApiOptions> options)
        {
            _tokenService = tokenService;
            _factory = factory;
            _options = options;
        }

        [FunctionName("Proxy")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, Route = "Proxy/{*path}")] HttpRequest req, string path)
        {
            var authHeader = req.Headers["Authorization"].ToString();
            var clientId = "";
            var clientSecret = "";
            if (authHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
            {
                // parse client credentials
                var credentialString = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Substring("Basic ".Length).Trim()));
                var credentials = credentialString.Split(':');

                if(credentials.Length != 2)
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent("Incorrect Basic authentication header format.")
                    };
                }
                clientId = credentials[0];
                clientSecret = credentials[1];
            }
            else if(req.Query.ContainsKey("$clientid") && req.Query.ContainsKey("$clientSecret"))
            {
                req.Query.TryGetValue("$clientId", out var id);
                req.Query.TryGetValue("$clientSecret", out var secret);

                clientId = id[0];
                clientSecret = secret[0];
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Specify client credentials either as Basic auth or in Query.")
                };
            }
            try
            {
                // acquire bearer token
                var token = await _tokenService.GetToken(clientId, clientSecret);

                // send request with previously acquired bearer token
                var client = new FluentClient(new Uri(_options.Value.BaseUrl), _factory.CreateClient());
                var builder = new UriBuilder(_options.Value.BaseUrl)
                {
                    Path = path,
                    Query = req.QueryString.Value,
                    Port = -1
                };
                var resource = builder.ToString();

                var message = new HttpRequestMessage(new HttpMethod(req.Method), resource);
                message.Content = new StreamContent(req.Body);
                foreach (var header in req.Headers)
                {
                    if (!message.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                    {
                        message.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }
                message.Headers.Host = builder.Host;

                //send request with previously acquired bearer token
                return await client.SendAsync(message)
                    .WithBearerAuthentication(token)
                    .AsMessage();
            }
            catch (ApiException ex)
            {
                // Forward any API error
                return ex.Response.Message;
            }
        }
    }
}
