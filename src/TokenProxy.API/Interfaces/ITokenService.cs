using System.Threading.Tasks;

namespace TokenProxy.API.Interfaces
{
    public interface ITokenService
    {
        Task<string> GetToken(string clientId, string clientSecret);
    }
}
