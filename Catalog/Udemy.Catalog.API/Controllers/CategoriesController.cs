using Microsoft.AspNetCore.Mvc;
using Udemy.Catalog.API.Dtos;
using Udemy.Catalog.API.Services.Abstract;

namespace Udemy.Catalog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET api/categories
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _categoryService.GetAllAsync();

            if (response == null || !response.Any())
                return NotFound(new { Message = "No categories found." });

            return Ok(response);
        }

        // GET api/categories/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var response = await _categoryService.GetByIdAsync(id);

            if (response == null)
                return NotFound(new { Message = "Category not found." });

            return Ok(response);
        }

        // POST api/categories
        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateDto categoryDto)
        {
            var createdCategory = await _categoryService.CreateAsync(categoryDto);

            if (createdCategory == null)
                return BadRequest(new { Message = "Category could not be created." });

            return CreatedAtAction(nameof(GetById), new { id = createdCategory.Id }, createdCategory);
        }
    }
}
