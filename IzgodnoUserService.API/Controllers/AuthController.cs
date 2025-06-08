using IzgodnoUserService.DTO.Auth;
using IzgodnoUserService.Services.AuthenticationServices.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IzgodnoUserService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        
        public AuthController (IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.IdToken))
                return BadRequest("No token provided");

            var result = await _authService.AuthenticateWithGoogleAsync(dto.IdToken);
            if (!result.Success)
                return BadRequest(result.Errors);

            _authService.SetJwtCookie(Response, result.Token!);
            return Ok(new { success = true });
        }

        [HttpGet("check-auth")]
        public IActionResult CheckAuth()
        {
            return User.Identity?.IsAuthenticated == true ? Ok() : Unauthorized();
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(new { success = true });
        }
    }
}
