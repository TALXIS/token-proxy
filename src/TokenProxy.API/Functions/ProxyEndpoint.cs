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
            if (authHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
            {
                try
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

                    // acquire bearer token
                    var first = credentials[0];
                    var second = credentials[1];
                    var token = await _tokenService.GetToken(first, second);

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
                    foreach (var header in req.Headers)
                    {
                        message.Headers.Add(header.Key, header.Value.ToArray());
                    }
                    message.Headers.Host = builder.Host;
                    message.Content = new StreamContent(req.Body);

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
            else
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Specify client credentials.")
                };
            }
        }
    }
}
