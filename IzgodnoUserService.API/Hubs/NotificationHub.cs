using IzgodnoUserService.Services.MessageQueueService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace IzgodnoUserService.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly RedisConnectionManager _redis;

        public NotificationHub(RedisConnectionManager redis)
        {
            _redis = redis;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await _redis.StoreConnectionIdAsync(userId, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await _redis.RemoveConnectionIdAsync(userId);
            }

            await base.OnDisconnectedAsync(ex);
        }
    }
}
