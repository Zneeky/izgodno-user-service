using IzgodnoUserService.API.Hubs;
using IzgodnoUserService.DTO.MessageModels;
using IzgodnoUserService.Services.MessageQueueService;
using IzgodnoUserService.Services.MessageQueueService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

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
        public async Task<IActionResult> LookupProduct([FromBody] ProductLookupRequestClient requestFromClient)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var request = new ProductLookupRequest(
                Guid.NewGuid(),
                userId,
                requestFromClient.ProductName,
                requestFromClient.Source,
                DateTime.UtcNow
            );

            var connectionId = requestFromClient.ConnectionId; // Or pass it from client via headers or query param

            // NEW: Store requestId → connectionId
            await _redis.SetRequestConnectionMappingAsync(request.RequestId.ToString(), connectionId);

            await _publisher.PublishProductLookup(request);

            return Ok(new { status = "published", requestId = request.RequestId });
        }
    }
}
