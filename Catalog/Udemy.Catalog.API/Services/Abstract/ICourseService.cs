using Udemy.Catalog.API.Dtos;

namespace Udemy.Catalog.API.Services.Abstract
{
    public interface ICourseService
    {
        Task<List<CourseDto>> GetAllAsync();
        Task<CourseDto> GetByIdAsync(string id);
        Task<List<CourseDto>> GetAllByUserIdAsync(string userId);
        Task<CourseDto> CreateAsync(CourseCreateDto courseCreateDto);
        Task<bool> UpdateAsync(CourseUpdateDto courseUpdateDto);
        Task<bool> DeleteAsync(string id);
    }
}
