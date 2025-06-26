using IzgodnoUserService.DTO.Auth;
using IzgodnoUserService.Services.AuthenticationServices.Interfaces;
using Microsoft.AspNetCore.Identity;
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
            _authService.SetRefreshTokenCookie(Response, result.RefreshToken!);
            return Ok(new { success = true });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var (success, newJwt, newRefresh) = await _authService.RefreshTokensAsync(refreshToken);
            if (!success)
                return Unauthorized();

            _authService.SetJwtCookie(Response, newJwt!);
            _authService.SetRefreshTokenCookie(Response, newRefresh!.Token);

            return Ok(new { token = newJwt });
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
