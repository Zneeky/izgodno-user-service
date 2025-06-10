using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IzgodnoUserService.DTO.MessageModels
{
    public class ProductResultDto
    {
        public string UserId { get; set; } = default!;
        public Guid RequestId { get; set; }
        public string Title { get; set; } = default!;
        public List<ProductOfferDto> Offers { get; set; } = new();
    }
}
