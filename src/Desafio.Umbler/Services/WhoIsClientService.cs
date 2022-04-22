using Desafio.Umbler.Interfaces;
using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Services
{
    public class WhoisClientService : IWhoisClient
    {
        public async Task<WhoisResponse> QueryAsync(string ip)
        {
            return await WhoisClient.QueryAsync(ip);
        }
    }
}
