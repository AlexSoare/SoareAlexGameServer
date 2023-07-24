namespace SoareAlexGameServer.Infrastructure.Interfaces
{
    public interface ICacheService<T>
    {
        T GetItem(string cacheKey);
        void DeleteItem(string cacheKey);
        void SetCachedItem(string cacheKey, T entry);
    }
}
