using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PontoDaFarra.DTOs.Location;
using PontoDaFarra.Exceptions;
using PontoDaFarra.Filters;
using PontoDaFarra.services.Interface;

namespace PontoDaFarra.Controllers
{
    [ApiController]
    [Route("locations")]
    [Authorize]
    public class LocationController(ILocationService locationService) : ControllerBase
    {
        [HttpPost]
        [ApiKey]
        public async Task<IActionResult> Create([FromBody] CreateLocationDto dto)
        {
            LocationResponseDto result = await locationService.CreateLocationAsync(GetUserId(), dto);
            return CreatedAtAction(nameof(GetById), new { locationId = result.LocationId }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<LocationResponseDto> result = await locationService.GetLocationsAsync(GetUserId());
            return Ok(result);
        }

        [HttpGet("{locationId:guid}")]
        public async Task<IActionResult> GetById(Guid locationId)
        {
            LocationResponseDto result = await locationService.GetLocationByIdAsync(GetUserId(), locationId);
            return Ok(result);
        }

        [HttpPatch("{locationId:guid}")]
        [ApiKey]
        public async Task<IActionResult> Update(Guid locationId, [FromBody] UpdateLocationDto dto)
        {
            LocationResponseDto result = await locationService.UpdateLocationAsync(GetUserId(), locationId, dto);
            return Ok(result);
        }

        [HttpDelete("{locationId:guid}")]
        [ApiKey]
        public async Task<IActionResult> Delete(Guid locationId)
        {
            await locationService.DeleteLocationAsync(GetUserId(), locationId);
            return NoContent();
        }

        private Guid GetUserId()
        {
            string? claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(claim, out Guid userId))
                throw new UnauthorizedException("Invalid access token");

            return userId;
        }
    }
}
