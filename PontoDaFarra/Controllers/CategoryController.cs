using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PontoDaFarra.DTOs.Category;
using PontoDaFarra.DTOs.Global;
using PontoDaFarra.services.Interface;
using PontoDaFarra.Filters;

namespace PontoDaFarra.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController(ICategoryServices categoryServices) : ControllerBase
    {
        [HttpPost]
        [ApiKey]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest dto)
        {
            CategoryResponse response = await categoryServices.CreateCategoryAsync(dto);
            return StatusCode(201, response);
        }

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetCategoryById(Guid categoryId)
        {
            CategoryResponse? response = await categoryServices.GetCategoryByIdAsync(categoryId);

            if (response is null)
                return NotFound();

            return Ok(response);
        }
        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetCategoryByName(string name)
        {
            CategoryResponse? response = await categoryServices.GetCategoryByNameAsync(name);

            if (response is null)
                return NotFound();

            return Ok(response);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCategories()
        {
            List<CategoryResponse> response = await categoryServices.GetAllCategoriesAsync();
            return Ok(response);
        }

        [HttpGet("with-products")]
        public async Task<ActionResult<PagedResponse<CategoryWithProductsResponse>>> GetAllWithProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        ){
            return Ok(await categoryServices.GetCategoriesWithProductsPagedAsync(page, pageSize));
        }

        [HttpPut("{categoryId}")]
        [ApiKey]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(Guid categoryId, [FromBody] CreateCategoryRequest dto)
        {
            try
            {
                CategoryResponse response = await categoryServices.UpdateCategoryAsync(categoryId, dto);
                return Ok(response);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
        [HttpDelete("{categoryId}")]
        [ApiKey]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(Guid categoryId)
        {
            try
            {
                CategoryResponse response = await categoryServices.DeleteCategoryAsync(categoryId);
                return Ok(response);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

    }
}