using IzgodnoUserService.API.Hubs;
using IzgodnoUserService.DTO.MessageModels;
using IzgodnoUserService.Services.MessageQueueService;
using IzgodnoUserService.Services.MessageQueueService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace IzgodnoUserService.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IRabbitMqPublisher _publisher;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly RedisConnectionManager _redis;

        public ProductController(IRabbitMqPublisher publisher, IHubContext<NotificationHub> hubContext, RedisConnectionManager redis)
        {
            _publisher = publisher;
            _hubContext = hubContext;
            _redis = redis;
        }

        [HttpPost("lookup")]
        public async Task<IActionResult> LookupProduct([FromBody] ProductLookupRequest requestFromClient)
        {
            var userId = User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var request = new ProductLookupRequest(
                Guid.NewGuid(),
                userId,
                requestFromClient.ProductName,
                requestFromClient.Source,
                DateTime.UtcNow
            );

            await _publisher.PublishProductLookup(request);

            return Ok(new { status = "published", requestId = request.RequestId });
        }

        [HttpPost("result")]
        public async Task<IActionResult> ReceiveResult([FromBody] ProductResultDto result)
        {
            var connectionId = await _redis.GetConnectionIdAsync(result.UserId);
            if (connectionId != null)
            {
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ReceiveProductResult", result);
            }

            return Ok();
        }
    }
}
