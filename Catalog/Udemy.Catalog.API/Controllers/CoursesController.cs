using Microsoft.AspNetCore.Mvc;
using Udemy.Catalog.API.Dtos;
using Udemy.Catalog.API.Services.Abstract;

namespace Udemy.Catalog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        // GET: api/courses
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _courseService.GetAllAsync();

            if (result == null || !result.Any())
                return NotFound(new { Message = "No courses found." });

            return Ok(result);
        }

        // GET: api/courses/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _courseService.GetByIdAsync(id);

            if (result == null)
                return NotFound(new { Message = "Course not found." });

            return Ok(result);
        }

        // GET: api/courses/GetAllByUserId/123
        [HttpGet("GetAllByUserId/{userId}")]
        public async Task<IActionResult> GetAllByUserId(string userId)
        {
            var result = await _courseService.GetAllByUserIdAsync(userId);

            if (result == null || !result.Any())
                return NotFound(new { Message = "No courses found for this user." });

            return Ok(result);
        }

        // POST: api/courses
        [HttpPost]
        public async Task<IActionResult> Create(CourseCreateDto dto)
        {
            var created = await _courseService.CreateAsync(dto);

            if (created == null)
                return BadRequest(new { Message = "Course could not be created." });

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/courses
        [HttpPut]
        public async Task<IActionResult> Update(CourseUpdateDto dto)
        {
            var updated = await _courseService.UpdateAsync(dto);

            if (updated == null)
                return NotFound(new { Message = "Course could not be updated." });

            return Ok(updated);
        }

        // DELETE: api/courses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _courseService.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { Message = "Course could not be deleted." });

            return NoContent();
        }
    }
}
