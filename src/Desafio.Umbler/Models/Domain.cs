using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Desafio.Umbler.Models
{
    public class Domain
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Ip { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string WhoIs { get; set; }
        public int Ttl { get; set; }
        public string HostedAt { get; set; }

        public Domain()
        {

        }

        public Domain(string domainName)
        {
            if(!isValid(domainName))
                throw new ArgumentException(nameof(domainName), "Domínio inválido");

            Name = domainName;
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
