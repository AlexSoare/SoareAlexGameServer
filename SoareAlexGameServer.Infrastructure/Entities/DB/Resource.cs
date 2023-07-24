using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoareAlexGameServer.Infrastructure.Entities.DB
{
    public enum ResourceType
    {
        Coins,
        Rolls
    }
    public class Resource
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResourceId { get; set; }

        public ResourceType ResourceType { get; set; }
        public double Value { get; set; }

        public void Update(double newAmount)
        {
            Value = newAmount;
        }
        public void Add(double amount)
        {
            Value += amount;
        }
        public void Remove(double amount)
        {
            Value -= amount;
        }
    }
}
