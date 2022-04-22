using System.ComponentModel.DataAnnotations;

namespace Desafio.Umbler.DTOs
{
    public class DomainDTO
    {
        public string Name { get; set; }
        public string Ip { get; set; }
        public string WhoIs { get; set; }
        public string HostedAt { get; set; }
    }
}
