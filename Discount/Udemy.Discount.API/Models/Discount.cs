namespace Udemy.Discount.API.Models
{
    [Dapper.Contrib.Extensions.Table("discount")]
    public class Discount
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int Rate { get; set; }
        public string Code { get; set; }
        public DateTime CreatedTime { get; set; }
        
        public DateTime? ExpirationDate { get; set; }
        
        /// <summary>
        /// Comma separated course ids
        /// </summary>
        public string? AllowedCourseIds { get; set; }
    }
}
