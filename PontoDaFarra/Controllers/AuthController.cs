using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PontoDaFarra.DTOs.Auth;
using PontoDaFarra.DTOs.User;
using PontoDaFarra.services;

namespace PontoDaFarra.controllers
{

    [ApiController]
    [Route("auth")]
    public class AuthController(
        UserServices userServices,
        AuthServices authServices,
        AuthCookieService authCookieService) : ControllerBase
    {

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserCreate dto)
        {
            try
            {
                var result = await userServices.CreateUserAsync(dto);

                authCookieService.Set(HttpContext, result.AccessToken, result.RefreshToken);

                return StatusCode(201, new { result.UserId, result.Email, result.Name, result.Role });
            }
            catch (InvalidOperationException)
            {
                return Conflict(new { message = "Email already registered" });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await authServices.LoginAsync(request);

            if (result is null)
                return Unauthorized(new { message = "Invalid email or password" });

            authCookieService.Set(HttpContext, result.AccessToken, result.RefreshToken);

            return Ok(new { message = "Authenticated" });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(RefreshRequest? request)
        {
            var refreshToken = Request.Cookies[AuthCookieService.RefreshTokenCookie] ?? request?.RefreshToken;

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Missing refresh token" });

            var result = await authServices.RefreshAsync(refreshToken);

            if (result is null)
                return Unauthorized(new { message = "Invalid or expired refresh token" });

            authCookieService.Set(HttpContext, result.AccessToken, result.RefreshToken);

            return Ok(new { message = "Token refreshed" });
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(RefreshRequest? request)
        {
            var refreshToken = Request.Cookies[AuthCookieService.RefreshTokenCookie] ?? request?.RefreshToken;

            if (!string.IsNullOrEmpty(refreshToken))
                await authServices.LogoutAsync(refreshToken);

            authCookieService.Delete(HttpContext);

            return NoContent();
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                         ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Ok(new
            {
                userId,
                email = User.FindFirstValue(JwtRegisteredClaimNames.Email)
                        ?? User.FindFirstValue(ClaimTypes.Email),
                role = User.FindFirstValue(ClaimTypes.Role)
            });
        }
    }
}
