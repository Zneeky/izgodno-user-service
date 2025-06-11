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

        public async Task SetRequestConnectionMappingAsync(string requestId, string connectionId)
        {
            await _db.StringSetAsync($"request:{requestId}", connectionId, TimeSpan.FromMinutes(5));
        }

        public async Task<string?> GetConnectionIdByRequestIdAsync(string requestId)
        {
            return await _db.StringGetAsync($"request:{requestId}");
        }

        public async Task RemoveRequestConnectionMappingAsync(string requestId)
        {
            await _db.KeyDeleteAsync($"request:{requestId}");
        }
    }
}
