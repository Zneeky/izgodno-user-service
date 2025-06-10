using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IzgodnoUserService.DTO.MessageModels
{
    public class ProductOfferDto
    {
        public string Website { get; set; } = default!;
        public decimal Price { get; set; }
        public string ProductPageUrl { get; set; } = default!;
    }
}
