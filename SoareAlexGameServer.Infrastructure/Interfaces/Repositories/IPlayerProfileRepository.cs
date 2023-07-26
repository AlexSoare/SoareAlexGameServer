using SoareAlexGameServer.Infrastructure.Entities.DB;

namespace SoareAlexGameServer.Infrastructure.Interfaces.Repositories
{
    public interface IPlayerProfileRepository : IRepository<PlayerProfile>
    {
        Task<PlayerProfile> GetItemByPlayerIdAsync(string playerId);
    }
}
