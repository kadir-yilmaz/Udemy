using Microsoft.Extensions.Options;
using Udemy.WebUI.Settings;

namespace Udemy.WebUI.Helpers
{
    public class PhotoHelper
    {
        private readonly ServiceApiSettings _serviceApiSettings;

        public PhotoHelper(IOptions<ServiceApiSettings> serviceApiSettings)
        {
            _serviceApiSettings = serviceApiSettings.Value;
        }

        public string? GetPhotoStockUrl(string? photoUrl)
        {
            if (string.IsNullOrEmpty(photoUrl))
                return null;
            
            // If it's already a full URL, return it
            if (photoUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return photoUrl;
            }
            
            // Fix path separator if needed
            photoUrl = photoUrl.Replace("\\", "/");

            var baseUri = (_serviceApiSettings.PhotoStockPublicUri ?? _serviceApiSettings.PhotoStockUri).TrimEnd('/');
            var path = photoUrl.TrimStart('/');

            // If path doesn't start with photos/, add the prefix
            if (!path.StartsWith("photos/", StringComparison.OrdinalIgnoreCase))
            {
                path = $"photos/{path}";
            }

            return $"{baseUri}/{path}";
        }
    }
}
