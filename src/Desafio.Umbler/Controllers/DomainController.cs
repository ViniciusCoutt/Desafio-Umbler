using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Desafio.Umbler.Models;
using Microsoft.EntityFrameworkCore;
using DnsClient;
using Desafio.Umbler.Data;
using Desafio.Umbler.Interfaces;
using Desafio.Umbler.DTOs;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Desafio.Umbler.Controllers
{
    [ApiController]
    [Route("api")]
    public class DomainController : ControllerBase
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
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Get(string domainName)
        {

            Domain domain = new Domain(domainName);
            domain = await _db.Domains.FirstOrDefaultAsync(d => d.Name == domainName);

            if (domain == null)
            {
                domain = new Domain(domainName);
                var domainData = await RequestData(domain);
                _db.Domains.Add(domainData);
                await _db.SaveChangesAsync();
            }

            var needUpdate = DateTime.Now.Subtract(domain.UpdatedAt).TotalMinutes > domain.Ttl;
            if (needUpdate)
            {
                domain = await RequestData(domain);
                _db.Domains.Update(domain);
                await _db.SaveChangesAsync();
            }

            DomainDTO dto = new DomainDTO
            {
                Name = domain.Name,
                Ip = domain.Ip,
                WhoIs = domain.WhoIs,
                HostedAt = domain.HostedAt,
            };
            return Ok(dto);
        }

        private async Task<Domain> RequestData(Domain domain)
        {
            var response = await _whoisClient.QueryAsync(domain.Name);
            var result = await _lookupClient.QueryAsync(domain.Name, QueryType.A);
            var record = result.Answers.ARecords().FirstOrDefault();
            var address = record?.Address;

            var ip = address?.ToString();
            var hostResponse = await _whoisClient.QueryAsync(ip);

            var ttl = record?.TimeToLive ?? 0;
            var hostedAt = hostResponse.OrganizationName;
            var whoIs = response.Raw;

            domain.setDomain
            (
                ip,
                whoIs,
                ttl,
                hostedAt
            );

            return domain;
        }
    }
}
