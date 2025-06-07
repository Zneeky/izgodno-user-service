using IzgodnoUserService.Data.Models.UserEntities;
using IzgodnoUserService.Data.Models.UserEntities.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IzgodnoUserService.DTO.Mappers
{
    public static class UserMapper
    {
        public static UserDto ToDto(AppUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                CreatedAt = user.CreatedAt,
                UserTier = user.UserTier.ToString()
            };
        }

        public static AppUser ToEntity(UserDto dto)
        {
            return new AppUser
            {
                Id = dto.Id,
                Email = dto.Email,
                DisplayName = dto.DisplayName,
                UserTier = (UserTier)Enum.Parse(typeof(UserTier), dto.UserTier),
                CreatedAt = dto.CreatedAt
            };
        }
    }
}
