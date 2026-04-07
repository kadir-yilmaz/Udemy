using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Udemy.Catalog.API.Models;

namespace Udemy.Catalog.API.Dtos
{
    public class CourseDto
    {

        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public string UserId { get; set; }
        public string? UserName { get; set; }
        public string? Picture { get; set; }

        public DateTime CreatedTime { get; set; }

        public FeatureDto Feature { get; set; }

        public string CategoryId { get; set; }

        public CategoryDto Category { get; set; }
    }
}
