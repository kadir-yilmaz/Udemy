using System.Collections.Generic;
using Udemy.WebUI.Models.Catalogs;

namespace Udemy.WebUI.Models
{
    public class InstructorProfileViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; } = "Senior Software Developer | .NET Consultant"; // Placeholder
        public string Description { get; set; } // Placeholder
        public int TotalStudents { get; set; } = 12054; // Placeholder
        public int TotalReviews { get; set; } = 412; // Placeholder
        public string ProfileImageUrl { get; set; } // Placeholder
        public List<CourseViewModel> Courses { get; set; }
    }
}
