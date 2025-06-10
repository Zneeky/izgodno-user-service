using IzgodnoUserService.DTO.MessageModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IzgodnoUserService.Services.MessageQueueService.Interfaces
{
    public interface IRabbitMqPublisher
    {
        public Task PublishProductLookup(ProductLookupRequest request);
    }
}
