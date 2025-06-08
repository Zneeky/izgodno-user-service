using Google.Apis.Auth;
using IzgodnoUserService.Data.Models.UserEntities;
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
using System.Text;
using System.Threading.Tasks;

namespace IzgodnoUserService.Services.AuthenticationServices
{
    public class AuthService : IAuthService
    {

        private readonly IConfiguration _config;
        private readonly UserManager<AppUser> _userManager;

        public AuthService(IConfiguration config, UserManager<AppUser> userManager)
        {
            _config = config;
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
                expires: DateTime.UtcNow.AddDays(7),
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
                Expires = DateTime.UtcNow.AddDays(7)
            });
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
                    DisplayName = payload.Name ?? payload.GivenName ?? payload.FamilyName ?? "Unknown User",
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

            // Generate JWT
            var jwt = GenerateJwtToken(user);

            return new AuthResultDto
            {
                Success = true,
                Token = jwt
            };
        }
    }
}
