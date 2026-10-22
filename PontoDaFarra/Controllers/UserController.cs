using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PontoDaFarra.DTOs.User;
using PontoDaFarra.Exceptions;
using PontoDaFarra.services.Interface;

namespace PontoDaFarra.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UserController(IUserService userServices) : ControllerBase
    {
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUser dto)
        {
            var userId = GetUserId();
            var updated = await userServices.UpdateUserAsync(userId, dto);
            return Ok(updated);
        }

        [HttpPut("{Userid:guid}")]
        public async Task<IActionResult> UpdateUserById(Guid Userid)
        {
            var callerId = GetUserId();

            var updated = await userServices.UpdateUserRoleAsync(callerId, Userid);
            return Ok(updated);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser()
        {
            var userId = GetUserId();
            await userServices.DeleteUserAsync(userId);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUserById(Guid id)
        {
            var callerId = GetUserId();

            if (id != callerId && !User.IsInRole("Admin"))
                throw new ForbiddenException("Only administrators can delete other accounts");

            await userServices.DeleteUserAsync(id);
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