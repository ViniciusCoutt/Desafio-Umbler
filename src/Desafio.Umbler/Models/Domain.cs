using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Desafio.Umbler.Models
{
    public class Domain
    {
        [Key]
        public int Id { get; private set; }
        [Required]
        public string Name { get; private set; }
        public string Ip { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public string WhoIs { get; private set; }
        public int Ttl { get; private set; }
        public string HostedAt { get; private set; }

        public Domain()
        {
            UpdatedAt = DateTime.Now;
        }

        public Domain(string domainName) : this()
        {
            if (!isValid(domainName))
                throw new BadHttpRequestException("Nome de domínio inválido", 400);
            Name = domainName;
        }

        public void setDomain(string ip, string whoIs, int ttl, string hostedAt)
        {
            Ip = ip;
            WhoIs = whoIs;
            Ttl = ttl;
            HostedAt = hostedAt;
        }

        public static bool isValid(string domainName)
        {
            if (domainName == null)
                throw new ArgumentNullException(nameof(domainName), "Nome do domínio requerido");

            var regex = new Regex(@"(?=^.{4,253}$)(^((?!-)[a-zA-Z0-9-]{1,63}(?<!-)\.)+[a-zA-Z]{2,63}$)");

            return regex.IsMatch(domainName);
        }
    }
}
