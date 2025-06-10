using IzgodnoUserService.DTO.MessageModels;
using IzgodnoUserService.Services.MessageQueueService.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IzgodnoUserService.Services.MessageQueueService
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly IBus _bus;

        public RabbitMqPublisher(IBus bus)
        {
            _bus = bus;
        }

        public async Task PublishProductLookup(ProductLookupRequest request)
        {
            await _bus.Publish(request);
        }
    }
}
