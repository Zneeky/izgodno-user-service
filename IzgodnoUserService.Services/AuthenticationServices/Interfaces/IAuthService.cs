using IzgodnoUserService.Data.Models.UserEntities;
using IzgodnoUserService.DTO.Auth;
using Microsoft.AspNetCore.Http;

namespace IzgodnoUserService.Services.AuthenticationServices.Interfaces
{
    public interface IAuthService
    {
        public string GenerateJwtToken(AppUser user);
        public void SetJwtCookie(HttpResponse response, string token);
        public Task<AuthResultDto> AuthenticateWithGoogleAsync(string idToken);
    }
}
