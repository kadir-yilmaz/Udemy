using Udemy.WebUI.Models;
using Udemy.WebUI.Models.PhotoStocks;
using Udemy.WebUI.Services.Abstract;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Udemy.WebUI.Services.Concrete
{
    public class PhotoStockService : IPhotoStockService
    {
        private readonly HttpClient _httpClient;

        public PhotoStockService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> DeletePhoto(string photoUrl)
        {
            var response = await _httpClient.DeleteAsync($"photos?photoUrl={photoUrl}");
            return response.IsSuccessStatusCode;
        }

        public async Task<PhotoViewModel?> UploadPhoto(IFormFile? photo, string? courseName = null)
        {
            if (photo == null || photo.Length <= 0)
            {
                Console.WriteLine("[PhotoStock] No photo provided");
                return null;
            }

            Console.WriteLine($"[PhotoStock] Uploading photo: {photo.FileName}, Size: {photo.Length}");

            var guid = Guid.NewGuid().ToString();
            var fileName = guid;

            if (!string.IsNullOrEmpty(courseName))
            {
                var slug = Helpers.FriendlyNameHelper.GetFriendlyTitle(courseName);
                // GUID-Slug format (e.g. "guid-python-course.jpg")
                fileName = $"{fileName}-{slug}";
            }

            fileName += Path.GetExtension(photo.FileName);

            using var ms = new MemoryStream();
            await photo.CopyToAsync(ms);

            var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(new ByteArrayContent(ms.ToArray()), "photo", fileName);

            var response = await _httpClient.PostAsync("photos", multipartContent);
            
            Console.WriteLine($"[PhotoStock] Response status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[PhotoStock] Error: {errorContent}");
                return null;
            }

            // Read raw response for debugging
            var rawResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[PhotoStock] Raw response: {rawResponse}");

            // Parse the response - PhotoStock API returns { url: "photos/xxx.jpg" } directly
            var photoResult = System.Text.Json.JsonSerializer.Deserialize<PhotoViewModel>(rawResponse);
            Console.WriteLine($"[PhotoStock] Parsed URL: {photoResult?.Url ?? "NULL"}");
            
            return photoResult;
        }
    }
}
