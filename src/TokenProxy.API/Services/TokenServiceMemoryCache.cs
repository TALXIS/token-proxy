using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using TokenProxy.API.Interfaces;
using TokenProxy.API.Options;

namespace TokenProxy.API.Services
{
    public class TokenServiceMemoryCache : ITokenService
    {
        private readonly ITokenService _decorated;
        private readonly IMemoryCache _cahce;
        private readonly IOptions<oAuth2Options> _options;

        public TokenServiceMemoryCache(ITokenService decorated, IMemoryCache cahce, IOptions<oAuth2Options> options)
        {
            _decorated = decorated;
            _cahce = cahce;
            _options = options;
        }

        public Task<string> GetToken(string clientId, string clientSecret) =>
            _cahce.GetOrCreateAsync((clientId, clientSecret), item =>
            {
                item.SetAbsoluteExpiration(_options.Value.TokenCacheTime);
                return _decorated.GetToken(clientId, clientSecret);
            });
    }
}
