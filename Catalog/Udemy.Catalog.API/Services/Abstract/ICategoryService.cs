using Udemy.Catalog.API.Dtos;

namespace Udemy.Catalog.API.Services.Abstract
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto> CreateAsync(CategoryCreateDto category);
        Task<CategoryDto> GetByIdAsync(string id);
    }

}
