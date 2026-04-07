using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Udemy.Catalog.API.Dtos;
using Udemy.Catalog.API.Models;
using Udemy.Catalog.API.Options;
using Udemy.Catalog.API.Services.Abstract;

namespace Udemy.Catalog.API.Services.Concrete
{
    public class CategoryService : ICategoryService
    {
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMapper _mapper;

        public CategoryService(IMapper mapper, IOptions<DatabaseOptions> databaseSettings)
        {
            var settings = databaseSettings.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _categoryCollection = database.GetCollection<Category>(settings.CategoryCollectionName);
            _mapper = mapper;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var categories = await _categoryCollection.Find(category => true).ToListAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> GetByIdAsync(string id)
        {
            var category = await _categoryCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

            if (category == null)
                return null;

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateAsync(CategoryCreateDto categoryDto)
        {
            var category = _mapper.Map<Category>(categoryDto);

            await _categoryCollection.InsertOneAsync(category);

            return _mapper.Map<CategoryDto>(category);
        }
    }
}
