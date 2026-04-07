using AutoMapper;
using MassTransit;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Udemy.Catalog.API.Dtos;
using Udemy.Catalog.API.Models;
using Udemy.Catalog.API.Options;
using Udemy.Catalog.API.Services.Abstract;
using Udemy.Shared.Events;

namespace Udemy.Catalog.API.Services.Concrete
{
    public class CourseService : ICourseService
    {
        private readonly IMongoCollection<Course> _courseCollection;
        private readonly IMongoCollection<Category> _categoryCollection;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPublishEndpoint _publishEndpoint;

        public CourseService(
            IMapper mapper, 
            IOptions<DatabaseOptions> databaseSettings, 
            IHttpContextAccessor httpContextAccessor,
            IPublishEndpoint publishEndpoint)
        {
            var settings = databaseSettings.Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _courseCollection = database.GetCollection<Course>(settings.CourseCollectionName);
            _categoryCollection = database.GetCollection<Category>(settings.CategoryCollectionName);
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<List<CourseDto>> GetAllAsync()
        {
            var courses = await _courseCollection.Find(_ => true).ToListAsync();

            if (courses.Any())
            {
                foreach (var course in courses)
                {
                    course.Category = await _categoryCollection.Find<Category>(x => x.Id == course.CategoryId).FirstOrDefaultAsync();
                }
            }
            else
            {
                courses = new List<Course>();
            }

            return _mapper.Map<List<CourseDto>>(courses);
        }

        public async Task<CourseDto> GetByIdAsync(string id)
        {
            var course = await _courseCollection.Find<Course>(x => x.Id == id).FirstOrDefaultAsync();

            if (course == null)
            {
                return null;
            }
            course.Category = await _categoryCollection.Find<Category>(x => x.Id == course.CategoryId).FirstOrDefaultAsync();

            return _mapper.Map<CourseDto>(course);
        }

        public async Task<List<CourseDto>> GetAllByUserIdAsync(string userId)
        {
            var courses = await _courseCollection.Find<Course>(x => x.UserId == userId).ToListAsync();

            if (courses.Any())
            {
                foreach (var course in courses)
                {
                    course.Category = await _categoryCollection.Find<Category>(x => x.Id == course.CategoryId).FirstOrDefaultAsync();
                }
            }
            else
            {
                courses = new List<Course>();
            }

            return _mapper.Map<List<CourseDto>>(courses);
        }

        public async Task<CourseDto> CreateAsync(CourseCreateDto dto)
        {
            var newCourse = _mapper.Map<Course>(dto);
            newCourse.CreatedTime = DateTime.Now;
            
            // Extract UserName from Token (ClaimTypes.Name)
            if (_httpContextAccessor.HttpContext != null)
            {
                var user = _httpContextAccessor.HttpContext.User;
                var nameClaim = user.FindFirst("name");

                newCourse.UserId = user.FindFirst("sub")?.Value ?? dto.UserId; // Fallback to DTO if sub is missing
                newCourse.UserName = nameClaim?.Value; // Assign UserName from 'name' claim
            }

            await _courseCollection.InsertOneAsync(newCourse);

            return _mapper.Map<CourseDto>(newCourse);
        }

        public async Task<bool> UpdateAsync(CourseUpdateDto dto)
        {
            // Mevcut kursu al, ad değişikliği kontrolü için
            var existingCourse = await _courseCollection.Find<Course>(x => x.Id == dto.Id).FirstOrDefaultAsync();
            
            var updateCourse = _mapper.Map<Course>(dto);

            var result = await _courseCollection.FindOneAndReplaceAsync(
                x => x.Id == dto.Id,
                updateCourse
            );

            // Kurs adı değiştiyse event fırlat
            if (result != null && existingCourse != null && existingCourse.Name != dto.Name)
            {
                await _publishEndpoint.Publish(new CourseNameChanged
                {
                    CourseId = dto.Id,
                    NewName = dto.Name
                });
                
                Console.WriteLine($"[CourseService] Published CourseNameChanged event for CourseId: {dto.Id}, NewName: {dto.Name}");
            }

            return result != null; // true = success, false = not found
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _courseCollection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }
    }
}

