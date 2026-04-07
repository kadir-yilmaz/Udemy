using Udemy.WebUI.Helpers;
using Udemy.WebUI.Models.Catalogs;
using Udemy.WebUI.Services.Abstract;
using System.Net.Http.Json;

namespace Udemy.WebUI.Services.Concrete
{
    public class CatalogService : ICatalogService
    {
        private readonly HttpClient _client;
        private readonly IPhotoStockService _photoStockService;
        private readonly PhotoHelper _photoHelper;

        public CatalogService(HttpClient client, IPhotoStockService photoStockService, PhotoHelper photoHelper)
        {
            _client = client;
            _photoStockService = photoStockService;
            _photoHelper = photoHelper;
        }

        public async Task<bool> CreateCourseAsync(CourseCreateInput courseCreateInput)
        {
            var resultPhotoService = await _photoStockService.UploadPhoto(courseCreateInput.PhotoFormFile, courseCreateInput.Name);

            if (resultPhotoService != null)
            {
                courseCreateInput.Picture = resultPhotoService.Url;
            }

            var response = await _client.PostAsJsonAsync("courses", courseCreateInput);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[CatalogService] CreateCourse failed: {response.StatusCode}");
                Console.WriteLine($"[CatalogService] Error details: {errorContent}");
            }
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCourseAsync(string courseId)
        {
            var response = await _client.DeleteAsync($"courses/{courseId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<CategoryViewModel>> GetAllCategoryAsync()
        {
            var response = await _client.GetAsync("categories");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[CatalogService] GetAllCategoryAsync failed with status: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<List<CategoryViewModel>>();
        }

        public async Task<List<CourseViewModel>> GetAllCourseAsync()
        {
            var response = await _client.GetAsync("courses");

            if (!response.IsSuccessStatusCode)
                return null;

            var courses = await response.Content.ReadFromJsonAsync<List<CourseViewModel>>();
            
            if (courses != null)
            {
                foreach (var course in courses)
                {
                    course.StockPictureUrl = _photoHelper.GetPhotoStockUrl(course.Picture);
                }
            }
            
            return courses ?? new List<CourseViewModel>();
        }

        public async Task<List<CourseViewModel>> GetAllCourseByUserIdAsync(string userId)
        {
            var response = await _client.GetAsync($"courses/GetAllByUserId/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[CatalogService] GetAllCourseByUserIdAsync failed: {response.StatusCode}");
                return null;
            }

            var courses = await response.Content.ReadFromJsonAsync<List<CourseViewModel>>();
            
            if (courses != null)
            {
                courses.ForEach(x => x.StockPictureUrl = _photoHelper.GetPhotoStockUrl(x.Picture));
            }

            return courses ?? new List<CourseViewModel>();
        }

        public async Task<CourseViewModel> GetByCourseId(string courseId)
        {
            var response = await _client.GetAsync($"courses/{courseId}");

            if (!response.IsSuccessStatusCode)
                return null;

            var course = await response.Content.ReadFromJsonAsync<CourseViewModel>();
            
            if (course != null)
            {
                course.StockPictureUrl = _photoHelper.GetPhotoStockUrl(course.Picture);
            }

            return course;
        }

        public async Task<bool> UpdateCourseAsync(CourseUpdateInput courseUpdateInput)
        {
            var resultPhotoService = await _photoStockService.UploadPhoto(courseUpdateInput.PhotoFormFile, courseUpdateInput.Name);

            if (resultPhotoService != null)
            {
                await _photoStockService.DeletePhoto(courseUpdateInput.Picture);
                courseUpdateInput.Picture = resultPhotoService.Url;
            }

            var response = await _client.PutAsJsonAsync("courses", courseUpdateInput);
            return response.IsSuccessStatusCode;
        }
    }
}

