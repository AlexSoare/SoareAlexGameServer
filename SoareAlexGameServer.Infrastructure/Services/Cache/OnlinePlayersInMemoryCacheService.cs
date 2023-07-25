using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using SoareAlexGameServer.Infrastructure.Entities;
using SoareAlexGameServer.Infrastructure.Entities.DB;
using SoareAlexGameServer.Infrastructure.Interfaces;
using SoareAlexGameServer.Infrastructure.Interfaces.Cache;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SoareAlexGameServer.Infrastructure.Services.Cache
{
    public class OnlinePlayersInMemoryCacheService : IOnlinePlayersCacheService
    {
        private readonly IMemoryCache _cache;
      
        public OnlinePlayersInMemoryCacheService(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public void SetCachedItem(string cacheKey, OnlinePlayer item)
        {
            _cache.Set(ComputeCacheKey(cacheKey), item);
        }

        public OnlinePlayer GetItem(string cacheKey)
        {
            OnlinePlayer item;
            _cache.TryGetValue(ComputeCacheKey(cacheKey), out item);

            return item;
        }

        public void DeleteItem(string cacheKey)
        {
            _cache.Remove(ComputeCacheKey(cacheKey));
        }

        private string ComputeCacheKey(string cacheKey)
        {
            return Constants.ONLINE_PLAYERS_CACHE_KEY + cacheKey;
        }
    }
}
