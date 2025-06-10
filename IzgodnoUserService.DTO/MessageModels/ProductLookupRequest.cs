using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IzgodnoUserService.DTO.MessageModels
{
    public record ProductLookupRequest(
        Guid? RequestId,
        string? UserId,
        string ProductName,
        string Source,
        DateTime Timestamp
    );
}
