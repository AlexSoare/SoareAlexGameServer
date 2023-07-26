using SoareAlexGameServer.Infrastructure.Data;
using SoareAlexGameServer.Infrastructure.Entities.DB;
using SoareAlexGameServer.Infrastructure.Interfaces.Repositories;

namespace SoareAlexGameServer.Infrastructure.Services.Repositories
{
    public class PlayerProfileSqliteRepository : IPlayerProfileRepository
    {
        private readonly SqliteDbContext dbContext;

        public PlayerProfileSqliteRepository(SqliteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddItemAsync(PlayerProfile item)
        {
            dbContext.Players.Add(item);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(string id)
        {
            var playerToRemove = dbContext.Players.Where(p => p.PlayerId == id);

            if (playerToRemove.Count() > 0)
                dbContext.Players.Remove(playerToRemove.First());

            await dbContext.SaveChangesAsync();
        }

        public async Task<PlayerProfile> GetItemAsync(string id)
        {
            var playerToReturn = dbContext.Players.Where(p => p.DeviceId == id);

            if (playerToReturn.Count() < 1)
                return null;

            return playerToReturn.First();
        }
        public async Task<PlayerProfile> GetItemByPlayerIdAsync(string playerId)
        {
            var playerToReturn = dbContext.Players.Where(p => p.PlayerId == playerId);

            if (playerToReturn.Count() < 1)
                return null;

            return playerToReturn.First();
        }

        public async Task UpdateItemAsync(string id, PlayerProfile item)
        {
            dbContext.Players.Update(item);
            await dbContext.SaveChangesAsync();
        }
    }
}
