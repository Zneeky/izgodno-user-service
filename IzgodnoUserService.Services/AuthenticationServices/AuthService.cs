using Google.Apis.Auth;
using IzgodnoUserService.Data.Models;
using IzgodnoUserService.Data.Models.UserEntities;
using IzgodnoUserService.Data.Repositories.Interfaces;
using IzgodnoUserService.DTO.Auth;
using IzgodnoUserService.Services.AuthenticationServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IzgodnoUserService.Services.AuthenticationServices
{
    public class AuthService : IAuthService
    {

        private readonly IConfiguration _config;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly UserManager<AppUser> _userManager;

        public AuthService(IConfiguration config, IRefreshTokenRepository refreshTokenRepository, UserManager<AppUser> userManager)
        {
            _config = config;
            _refreshTokenRepository = refreshTokenRepository;
            _userManager = userManager;
        }
        public string GenerateJwtToken(AppUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpireMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void SetJwtCookie(HttpResponse response, string token)
        {
            response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpireMinutes"]!)),
            });
        }

        public void SetRefreshTokenCookie(HttpResponse response, string token)
        {
            response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpireDays"]!)),
            });
        }

        public RefreshToken GenerateRefreshToken(Guid userId)
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpireDays"]!)),
                UserId = userId
            };
        }

        public async Task<AuthResultDto> AuthenticateWithGoogleAsync(string idToken)
        {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _config["GoogleAuthSettings:ClientId"] }
                });
            }
            catch
            {
                return new AuthResultDto
                {
                    Success = false,
                    Errors = new[] { "Invalid Google token." }
                };
            }

            // Check if user exists
            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
            {
                user = new AppUser
                {
                    Email = payload.Email,
                    UserName = payload.Email,
                    CreatedAt = DateTime.UtcNow,
                    DisplayName = payload.Name ?? payload.GivenName ?? payload.FamilyName ?? payload.Email,
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return new AuthResultDto
                    {
                        Success = false,
                        Errors = result.Errors.Select(e => e.Description)
                    };
                }
            }

            var jwt = GenerateJwtToken(user);
            var newRefresh = GenerateRefreshToken(user.Id);

            await _refreshTokenRepository.AddAsync(newRefresh);
            await _refreshTokenRepository.SaveChangesAsync();

            return new AuthResultDto
            {
                Success = true,
                Token = jwt,
                RefreshToken = newRefresh.Token
            };
        }

        public async Task<(bool Success, string? JwtToken, RefreshToken? RefreshToken)> RefreshTokensAsync(string refreshToken)
        {
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (storedToken == null ||
                storedToken.IsUsed ||
                storedToken.IsRevoked ||
                storedToken.Expires < DateTime.UtcNow)
            {
                return (false, null, null);
            }

            storedToken.IsUsed = true;
            await _refreshTokenRepository.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
            if (user == null)
            {
                return (false, null, null);
            }

            var newJwt = GenerateJwtToken(user);
            var newRefresh = GenerateRefreshToken(user.Id);

            await _refreshTokenRepository.AddAsync(newRefresh);
            await _refreshTokenRepository.SaveChangesAsync();

            return (true, newJwt, newRefresh);
        }
    }
}
