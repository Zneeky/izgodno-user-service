using IzgodnoUserService.Data.Models.UserEntities.Enumerators;
using Microsoft.AspNetCore.Identity;

namespace IzgodnoUserService.Data.Models.UserEntities
{
    public class AppUser : IdentityUser<Guid>
    {
        public AppUser()
        {
            CreatedAt = DateTime.UtcNow;
            UserTier = UserTier.Free;
            RefreshTokens = new List<RefreshToken>();
        }

        public DateTime CreatedAt { get; set; }

        public UserTier UserTier { get; set; }

        public string DisplayName { get; set; } = null!;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = null!;
    }
}
