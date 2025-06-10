using StackExchange.Redis;

namespace IzgodnoUserService.Services.MessageQueueService
{
    public class RedisConnectionManager
    {
        private readonly IDatabase _db;

        public RedisConnectionManager(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task StoreConnectionIdAsync(string userId, string connectionId)
        {
            await _db.StringSetAsync($"conn:{userId}", connectionId, TimeSpan.FromHours(1));
        }

        public async Task<string?> GetConnectionIdAsync(string userId)
        {
            return await _db.StringGetAsync($"conn:{userId}");
        }

        public async Task RemoveConnectionIdAsync(string userId)
        {
            await _db.KeyDeleteAsync($"conn:{userId}");
        }
    }
}
