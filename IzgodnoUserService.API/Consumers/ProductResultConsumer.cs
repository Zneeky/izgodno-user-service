using IzgodnoUserService.API.Hubs;
using IzgodnoUserService.DTO.MessageModels;
using IzgodnoUserService.Services.MessageQueueService;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace IzgodnoUserService.API.Consumers
{
    public class ProductResultConsumer : IConsumer<ProductResultDto>
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly RedisConnectionManager _redis;

        public ProductResultConsumer(IHubContext<NotificationHub> hubContext, RedisConnectionManager redis)
        {
            _hubContext = hubContext;
            _redis = redis;
        }

        public async Task Consume(ConsumeContext<ProductResultDto> context)
        {
            var result = context.Message;

            var connectionId = await _redis.GetConnectionIdAsync(result.UserId);
            if (connectionId != null)
            {
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ReceiveProductResult", result);
            }
            else
            {
                Console.WriteLine($"User not connected: {result.UserId}");
            }
        }
    }
}
