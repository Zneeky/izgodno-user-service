using IzgodnoUserService.DTO.Auth;
using IzgodnoUserService.Services.AuthenticationServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IzgodnoUserService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        
        public AuthController (AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            var result = await _authService.AuthenticateWithGoogleAsync(dto.IdToken);

            if (!result.Success)
                return BadRequest(result.Errors);

            // Set cookie (for extension compatibility)
            _authService.SetJwtCookie(Response, result.Token!);

            return Ok(new { success = true });
        }
    }
}
