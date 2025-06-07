using IzgodnoUserService.Data.Models.UserEntities;
using IzgodnoUserService.Data.Repositories.Interfaces;
using IzgodnoUserService.DTO;
using IzgodnoUserService.DTO.Mappers;
using IzgodnoUserService.Services.UserServices.Interfaces;

namespace IzgodnoUserService.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            if (user is null) return null;

            return UserMapper.ToDto(user);
        }
    }
}
