using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IzgodnoUserService.DTO.Auth
{
    public class GoogleLoginDto
    {
        public string IdToken { get; set; } = default!;
    }
}
