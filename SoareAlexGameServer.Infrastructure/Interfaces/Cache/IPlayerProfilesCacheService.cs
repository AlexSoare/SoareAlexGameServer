using SoareAlexGameServer.Infrastructure.Entities.DB;

namespace SoareAlexGameServer.Infrastructure.Interfaces.Cache
{
    public interface IPlayerProfilesCacheService : ICacheService<PlayerProfile>
    {
        void SetCachedItem(string deviceId, string playerId, PlayerProfile entry);
        PlayerProfile GetItemByPlayerId(string playerId);
    }
}
