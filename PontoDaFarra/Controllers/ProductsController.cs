using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PontoDaFarra.DTOs.Products;
using PontoDaFarra.services;
using PontoDaFarra.services.Interface;
using PontoDaFarra.Filters;

namespace PontoDaFarra.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController(IProductService productService) : ControllerBase
    {
        [HttpPost]
        [ApiKey]
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
        public async Task<IActionResult> Search(
            [FromQuery] SearchQuery dto)
        {
            var result = await productService.SearchProductsAsync(dto);
            return Ok(result);
        }
        [HttpPut("{productId}")]
        [ApiKey]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid productId, [FromForm] CreateProductRequest dto)
        {
            var result = await productService.UpdateProductAsync(productId, dto);
            if (result is null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{productId}")]
        [ApiKey]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid productId)
        {
            var result = await productService.DeleteProductAsync(productId);
            if (result is null)
                return NotFound();

            return Ok(result);
        }
        [HttpPatch("{productId}/availability")]
        [ApiKey]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAvailability(Guid productId)
        {
            var result = await productService.UpdateIsAvailableAsync(productId);
            if (result is null)
                return NotFound();

            return Ok(result);
        }
        [HttpPatch("{productId}/promotional")]
        [ApiKey]
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