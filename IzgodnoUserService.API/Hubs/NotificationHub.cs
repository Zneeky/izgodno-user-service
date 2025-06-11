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
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        // Frontend will call this to get its connectionId
        public Task<string> GetConnectionId()
        {
            return Task.FromResult(Context.ConnectionId);
        }
    }
}
