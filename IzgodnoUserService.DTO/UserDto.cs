using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IzgodnoUserService.DTO
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public string UserTier { get; set; } = null!;

        public DateTime CreatedAt { get; set; } 
    }
}
