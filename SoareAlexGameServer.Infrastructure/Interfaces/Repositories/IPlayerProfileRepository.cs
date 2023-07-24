using SoareAlexGameServer.Infrastructure.Entities.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoareAlexGameServer.Infrastructure.Interfaces.Repositories
{
    public interface IPlayerProfileRepository : IRepository<PlayerProfile>
    {
        Task<PlayerProfile> GetItemByPlayerIdAsync(string playerId);
    }
}
