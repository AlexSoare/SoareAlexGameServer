using System.ComponentModel.DataAnnotations;

namespace SoareAlexGameServer.Infrastructure.Entities.DB
{
    public class PlayerProfile
    {
        [Key]
        public string DeviceId { get; set; }
        public string PlayerId { get; set; }
        public List<Resource> Resources { get; set; } = new List<Resource>();
    }
}
