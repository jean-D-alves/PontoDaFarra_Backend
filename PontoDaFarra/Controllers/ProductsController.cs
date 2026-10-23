using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PontoDaFarra.DTOs.Products;
using PontoDaFarra.services;
using PontoDaFarra.services.Interface;

namespace PontoDaFarra.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController(IProductService productService) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateProductRequest dto)
        {
            var result = await productService.CreateProductAsync(dto);
            return Created($"/api/products/{result.ProductId}", result);
        }
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetById(Guid productId)
        {
            var result = await productService.GetProductByIdAsync(productId);
            if (result is null)
                return NotFound();

            return Ok(result);
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await productService.SearchProductsAsync(name, page, pageSize);
            return Ok(result);
        }
        [HttpPut("{productId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid productId, [FromForm] CreateProductRequest dto)
        {
            var result = await productService.UpdateProductAsync(productId, dto);
            if (result is null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{productId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid productId)
        {
            var result = await productService.DeleteProductAsync(productId);
            if (result is null)
                return NotFound();

            return Ok(result);
        }
        [HttpPatch("{productId}/availability")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAvailability(Guid productId)
        {
            var result = await productService.UpdateIsAvailableAsync(productId);
            if (result is null)
                return NotFound();

            return Ok(result);
        }
        [HttpPatch("{productId}/promotional")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePromotional(Guid productId, [FromBody] PromotionalRequest isPromotional)
        {
            var result = await productService.UpdateIsPromotionalAsync(isPromotional, productId);
            if (result is null)
                return NotFound();
            return Ok(result);
        }
    }
}