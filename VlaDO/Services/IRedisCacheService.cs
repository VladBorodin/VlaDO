using System.Text.Json;
using StackExchange.Redis;

namespace VlaDO.Services
{
    public interface IRedisCacheService
    {
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task<T?> GetAsync<T>(string key);
        Task RemoveAsync(string key);
    }

    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _cache;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _cache = redis.GetDatabase();
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var jsonData = JsonSerializer.Serialize(value);
            await _cache.StringSetAsync(key, jsonData, expiration);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var jsonData = await _cache.StringGetAsync(key);
            return jsonData.HasValue ? JsonSerializer.Deserialize<T>(jsonData!) : default;
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.KeyDeleteAsync(key);
        }
    }
}