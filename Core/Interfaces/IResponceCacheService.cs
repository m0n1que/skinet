using System;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IResponceCacheService
    {
        Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeToLive);
        Task<string> GetCachedResponseAsync(string cacheKey);
    }
}