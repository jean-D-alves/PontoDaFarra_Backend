using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PontoDaFarra.DTOs.Global;
using PontoDaFarra.DTOs.Orders;
using PontoDaFarra.services.Interface;
using PontoDaFarra.Filters;

namespace PontoDaFarra.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrdersController(IOrderService orderService) : ControllerBase
    {
        [HttpPost]
        [ApiKey]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            var userId = GetUserId();
            var result = await orderService.CreateOrderAsync(userId, request);
            return StatusCode(201, result);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMy([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetUserId();
            var result = await orderService.GetMyOrdersAsync(userId, page, pageSize);
            return Ok(result);
        }

        [HttpDelete("{orderId}")]
        [ApiKey]
        [Authorize]
        public async Task<IActionResult> Delete(Guid orderId)
        {
            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");
            await orderService.DeleteOrderAsync(userId, isAdmin, orderId);
            return NoContent();
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await orderService.GetAllOrdersAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("admin/chart")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetChart()
        {
            var result = await orderService.GetChartAsync();
            return Ok(result);
        }

        [HttpGet("more-order")]
        public async Task<IActionResult> GetMoreOrder()
        {
            var result = await orderService.GetProductsMoreOrder();
            return Ok(result);
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                        ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (!Guid.TryParse(claim?.Value, out var userId))
                throw new Exceptions.UnauthorizedException("Invalid access token");
            return userId;
        }
    }
}
