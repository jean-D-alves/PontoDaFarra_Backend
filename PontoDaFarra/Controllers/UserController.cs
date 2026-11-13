using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PontoDaFarra.DTOs.User;
using PontoDaFarra.Exceptions;
using PontoDaFarra.services;
using PontoDaFarra.services.Interface;
using PontoDaFarra.Filters;

namespace PontoDaFarra.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UserController(
        IUserService userServices,
        AuthCookieService authCookieService) : ControllerBase
    {
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] UserFilterQuery query)
        {
            var result = await userServices.GetAllUsersAsync(query);
            return Ok(result);
        }

        [HttpGet("{userId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid userId)
        {
            var result = await userServices.GetUserByIdAsync(userId);
            return Ok(result);
        }

        [HttpPut]
        [ApiKey]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUser dto)
        {
            var userId = GetUserId();
            var updated = await userServices.UpdateUserAsync(userId, dto);
            return Ok(updated);
        }

        [HttpPut("{Userid:guid}")]
        [ApiKey]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserById(Guid Userid)
        {
            var callerId = GetUserId();

            var updated = await userServices.UpdateUserRoleAsync(callerId, Userid);
            return Ok(updated);
        }

        [HttpDelete]
        [ApiKey]
        public async Task<IActionResult> DeleteUser()
        {
            var userId = GetUserId();
            await userServices.DeleteUserAsync(userId);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ApiKey]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUserById(Guid id)
        {
            var callerId = GetUserId();

            if (id != callerId && !User.IsInRole("Admin"))
                throw new ForbiddenException("Only administrators can delete other accounts");

            await userServices.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpPut("me/password")]
        [ApiKey]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordRequest dto)
        {
            var userId = GetUserId();

            var result = await userServices.ChangeOwnPasswordAsync(userId, dto);

            // Sessões antigas foram revogadas: renova os cookies do próprio usuário.
            authCookieService.Set(HttpContext, result.AccessToken, result.RefreshToken);

            return Ok(new { message = "Password changed" });
        }

        [HttpPut("admin/{id:guid}/password")]
        [ApiKey]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminChangePassword(Guid id, [FromBody] AdminChangePasswordRequest dto)
        {
            var adminId = GetUserId();

            await userServices.AdminResetPasswordAsync(adminId, id, dto);

            return NoContent();
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(claim, out var userId))
                throw new UnauthorizedException("Invalid access token");

            return userId;
        }
    }
}