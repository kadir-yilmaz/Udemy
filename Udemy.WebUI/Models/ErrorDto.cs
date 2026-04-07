using System.Collections.Generic;

namespace Udemy.WebUI.Models
{
    /// <summary>
    /// API hata detayları için DTO
    /// </summary>
    public class ErrorDto
    {
        public List<string> Errors { get; set; }
    }
}
