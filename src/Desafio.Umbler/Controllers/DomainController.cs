using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Desafio.Umbler.Models;
using Microsoft.EntityFrameworkCore;
using DnsClient;
using Desafio.Umbler.Data;
using Desafio.Umbler.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Desafio.Umbler.Controllers
{
    [ApiController]
    [Route("api")]
    [AllowAnonymous]
    public class DomainController : Controller
    {
        private readonly DatabaseContext _db;
        private readonly ILookupClient _lookupClient;
        private readonly IWhoisClient _whoisClient;

        public DomainController(DatabaseContext db, ILookupClient lookupClient, IWhoisClient whoisClient)
        {
            _db = db ??
                throw new ArgumentNullException(nameof(db));

            _lookupClient = lookupClient ??
                throw new ArgumentNullException(nameof(lookupClient));

            _whoisClient = whoisClient ??
                throw new ArgumentNullException(nameof(whoisClient));
        }

        [HttpGet, Route("domain/{domainName}")]
        //[ProducesResponseType(typeof(IEnumerable<FaseGetResult>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(MbCoreException), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(InternalError), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(string domainName)
        {
            // Verifica se o domínio pesquisado já possui copia no banco de dados
            var domain = await _db.Domains.FirstOrDefaultAsync(d => d.Name == domainName);

            // Inicia se domain voltar nulo
            if (domain == null)
            {
                // Requisita as informações 
                var response = await _whoisClient.QueryAsync(domainName);
                var result = await _lookupClient.QueryAsync(domainName, QueryType.ANY);
                var record = result.Answers.ARecords().FirstOrDefault();
                var address = record?.Address;
                var ip = address?.ToString();

                var hostResponse = await _whoisClient.QueryAsync(ip);

                domain = new Domain
                {
                    Name = domainName,
                    Ip = ip,
                    UpdatedAt = DateTime.Now,
                    WhoIs = response.Raw,
                    Ttl = record?.TimeToLive ?? 0,
                    HostedAt = hostResponse.OrganizationName
                };
                // Adiciona o domain ao banco de dados
                _db.Domains.Add(domain);
            }

            // Verifica se as informações precisam ser atualizadas baseado no TTL
            var needUpdate = DateTime.Now.Subtract(domain.UpdatedAt).TotalMinutes > domain.Ttl;
            if (needUpdate)
            {
                var response = await _whoisClient.QueryAsync(domainName);
                var result = await _lookupClient.QueryAsync(domainName, QueryType.ANY);
                var record = result.Answers.ARecords().FirstOrDefault();
                var address = record?.Address;
                var ip = address?.ToString();

                var hostResponse = await _whoisClient.QueryAsync(ip);

                domain.Name = domainName;
                domain.Ip = ip;
                domain.UpdatedAt = DateTime.Now;
                domain.WhoIs = response.Raw;
                domain.Ttl = record?.TimeToLive ?? 0;
                domain.HostedAt = hostResponse.OrganizationName;
            }

            await _db.SaveChangesAsync();

            return Ok(domain);
        }
    }
}
