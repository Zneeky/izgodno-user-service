using IzgodnoUserService.Data.Models.UserEntities;
using IzgodnoUserService.Data.Repositories.Interfaces;
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

        public async Task<AppUser?> GetByEmailAsync(string email)
        {
            return await _userRepo.GetByEmailAsync(email);
        }
    }
}
