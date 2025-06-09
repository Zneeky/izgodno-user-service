using IzgodnoUserService.Data.Models;
using IzgodnoUserService.Data.Models.UserEntities;
using IzgodnoUserService.DTO.Auth;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace IzgodnoUserService.Services.AuthenticationServices.Interfaces
{
    public interface IAuthService
    {
        public string GenerateJwtToken(AppUser user);
        public void SetJwtCookie(HttpResponse response, string token);
        public void SetRefreshTokenCookie(HttpResponse response, string token);
        public RefreshToken GenerateRefreshToken(Guid userId);
        public Task<AuthResultDto> AuthenticateWithGoogleAsync(string idToken);
        public Task<(bool Success, string? JwtToken, RefreshToken? RefreshToken)> RefreshTokensAsync(string refreshToken);
    }
}
