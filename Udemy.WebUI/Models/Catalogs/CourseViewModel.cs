using System;
using System.Text.Json.Serialization;

namespace Udemy.WebUI.Models.Catalogs
{
    public class CourseViewModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        public string ShortDescription
        {
            get => string.IsNullOrEmpty(Description) ? "" : (Description.Length > 100 ? Description.Substring(0, 100) + "..." : Description);
        }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }
        
        [JsonPropertyName("picture")]
        public string? Picture { get; set; }

        [JsonIgnore]
        public string? StockPictureUrl { get; set; }

        [JsonPropertyName("createdTime")]
        public DateTime CreatedTime { get; set; }

        [JsonPropertyName("feature")]
        public FeatureViewModel Feature { get; set; }

        [JsonPropertyName("categoryId")]
        public string CategoryId { get; set; }

        [JsonPropertyName("category")]
        public CategoryViewModel Category { get; set; }
    }
}
