using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PontoDaFarra.DTOs.Legal;
using PontoDaFarra.Exceptions;
using PontoDaFarra.Filters;
using PontoDaFarra.services.Interface;

namespace PontoDaFarra.Controllers
{
    [ApiController]
    [Route("legal")]
    public class LegalController(ILegalService legalService) : ControllerBase
    {
        [HttpGet("{type}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActive(string type)
        {
            var result = await legalService.GetActiveByTypeAsync(type);
            return Ok(result);
        }

        [HttpGet("pending")]
        [Authorize]
        public async Task<IActionResult> GetPending()
        {
            var result = await legalService.GetPendingAsync(GetUserId());
            return Ok(result);
        }

        [HttpPost("accept")]
        [ApiKey]
        [Authorize]
        public async Task<IActionResult> Accept([FromBody] AcceptLegalDocumentsDto dto)
        {
            await legalService.AcceptAsync(GetUserId(), dto, GetIpAddress());
            return NoContent();
        }

        [HttpGet("consents")]
        [Authorize]
        public async Task<IActionResult> GetConsents()
        {
            var result = await legalService.GetConsentsAsync(GetUserId());
            return Ok(result);
        }

        [HttpGet("documents")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDocuments()
        {
            var result = await legalService.GetAllDocumentsAsync();
            return Ok(result);
        }

        [HttpPost("documents")]
        [ApiKey]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Publish([FromBody] PublishLegalDocumentDto dto)
        {
            var result = await legalService.PublishAsync(dto);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPatch("documents/{legalDocumentId:guid}")]
        [ApiKey]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            Guid legalDocumentId,
            [FromBody] UpdateLegalDocumentDto dto)
        {
            var result = await legalService.UpdateAsync(legalDocumentId, dto);
            return Ok(result);
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(claim, out var userId))
                throw new UnauthorizedException("Invalid access token");

            return userId;
        }

        private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
