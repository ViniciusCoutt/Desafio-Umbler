using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Interfaces
{
    public interface IWhoisClient
    {
        Task<WhoisResponse> QueryAsync(string ip);
    }
}
