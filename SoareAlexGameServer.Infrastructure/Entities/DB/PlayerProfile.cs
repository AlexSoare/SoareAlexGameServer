using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
