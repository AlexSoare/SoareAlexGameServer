namespace SoareAlexGameServer.Infrastructure.Interfaces.Repositories
{
    public interface IRepository<T>
    {
        Task<T> GetItemAsync(string id);
        Task AddItemAsync(T item);
        Task UpdateItemAsync(string id, T item);
        Task DeleteItemAsync(string id);
    }
}
