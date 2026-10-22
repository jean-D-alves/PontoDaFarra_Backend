using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PontoDaFarra.DTOs.Auth;
using PontoDaFarra.DTOs.User;
using PontoDaFarra.Exceptions;
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
            var result = await userServices.CreateUserAsync(dto);

            authCookieService.Set(HttpContext, result.AccessToken, result.RefreshToken);

            return StatusCode(201, new { result.UserId, result.Email, result.Name, result.PhoneNumber, result.Role });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await authServices.LoginAsync(request);

            authCookieService.Set(HttpContext, result.AccessToken, result.RefreshToken);

            return Ok(new { message = "Authenticated" });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(RefreshRequest? request)
        {
            var refreshToken = Request.Cookies[AuthCookieService.RefreshTokenCookie] ?? request?.RefreshToken;

            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedException("Missing refresh token");

            var result = await authServices.RefreshAsync(refreshToken);

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
                name = User.FindFirstValue(JwtRegisteredClaimNames.Name),
                email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? User.FindFirstValue(ClaimTypes.Email),
                phoneNumber = User.FindFirstValue(JwtRegisteredClaimNames.PhoneNumber) ?? User.FindFirstValue("phone_number"),
                role = User.FindFirstValue(ClaimTypes.Role)
            });
        }
    }
}
