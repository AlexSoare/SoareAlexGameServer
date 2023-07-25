using Microsoft.Extensions.Caching.Memory;
using SoareAlexGameServer.Infrastructure.Entities.DB;
using SoareAlexGameServer.Infrastructure.Interfaces.Cache;
using System.Collections.Concurrent;

namespace SoareAlexGameServer.Infrastructure.Services.Cache
{
    public class PlayerProfilesInMemoryCacheService : IPlayerProfilesCacheService
    {
        private const int CACHE_EXPIRY_IN_MINUTES = 60;

        private IMemoryCache _cache;

        private ConcurrentDictionary<string, string> PlayersIdsMappedToDeviceId;

        public PlayerProfilesInMemoryCacheService(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            PlayersIdsMappedToDeviceId = new ConcurrentDictionary<string, string>();
        }

        public void SetCachedItem(string cacheKey, PlayerProfile item)
        {
            throw new NotImplementedException();
        }

        public void SetCachedItem(string deviceId, string playerId, PlayerProfile item)
        {
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_EXPIRY_IN_MINUTES));

            PlayersIdsMappedToDeviceId.TryAdd(playerId, deviceId);

            _cache.Set(ComputeCacheKey(deviceId), item, cacheEntryOptions);
        }

        public PlayerProfile GetItemByPlayerId(string playerId)
        {
            var deviceId = "";

            if(PlayersIdsMappedToDeviceId.TryGetValue(playerId, out deviceId))
            {
                PlayerProfile item;
                _cache.TryGetValue(ComputeCacheKey(deviceId), out item);
                return item;
            }

            return null;
        }

        public PlayerProfile GetItem(string deviceId)
        {
            PlayerProfile item;
            _cache.TryGetValue(ComputeCacheKey(deviceId), out item);

            return item;
        }

        public void DeleteItem(string deviceId)
        {
            _cache.Remove(ComputeCacheKey(deviceId));
        }

        private string ComputeCacheKey(string cacheKey)
        {
            return Constants.PLAYERS_PROFILES_CACHE_KEY + cacheKey;
        }
    }
}
