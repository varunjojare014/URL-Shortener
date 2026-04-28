using StackExchange.Redis;

namespace UrlShortener.Services
{
    public class RedisService
    {
        private readonly IDatabase _db;

        public RedisService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<string?> GetAsync(string key)
        {
            var value = await _db.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }

        public async Task SetAsync(string key, string value)
        {
            await _db.StringSetAsync(key, value, TimeSpan.FromHours(1));
        }
    }
}