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

            var connectionId = await _redis.GetConnectionIdByRequestIdAsync(result.RequestId.ToString());
            if (connectionId != null)
            {
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ReceiveProductResult", result);

                await _redis.RemoveRequestConnectionMappingAsync(result.RequestId.ToString());
            }
            else
            {
                Console.WriteLine($"No connection ID found for request: {result.RequestId}");
            }
        }
    }
}
