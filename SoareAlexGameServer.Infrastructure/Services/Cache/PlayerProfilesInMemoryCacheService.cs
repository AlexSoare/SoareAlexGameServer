//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.IdentityModel.Tokens;
//using SoareAlexGameServer.Infrastructure.Entities;
//using SoareAlexGameServer.Infrastructure.Entities.DB;
//using SoareAlexGameServer.Infrastructure.Interfaces;
//using SoareAlexGameServer.Infrastructure.Interfaces.Cache;
//using System;
//using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;

//namespace SoareAlexGameServer.Infrastructure.Services.Cache
//{
//    public class PlayerProfilesInMemoryCacheService : IPlayerProfilesCacheService
//    {
//        private IMemoryCache _cache;
//        private int CACHE_EXPIRY_IN_MINUTES = 60;

//        public PlayerProfilesInMemoryCacheService(IMemoryCache cache)
//        {
//            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
//        }

//        public void SetCachedItem(string cacheKey, PlayerProfile item)
//        {
//            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_EXPIRY_IN_MINUTES));

//            _cache.Set(cacheKey, item, cacheEntryOptions);
//        }

//        public PlayerProfile GetItem(string cacheKey)
//        {
//            PlayerProfile item;
//            _cache.TryGetValue(cacheKey, out item);

//            return item;
//        }

//        public void DeleteItem(string cacheKey)
//        {
//            _cache.Remove(cacheKey);
//        }
//    }
//}
