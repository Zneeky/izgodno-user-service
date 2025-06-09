using IzgodnoUserService.Data.Models.UserEntities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IzgodnoUserService.Data.Models
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; } = false;
        public bool IsUsed { get; set; } = false;

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;
    }
}
