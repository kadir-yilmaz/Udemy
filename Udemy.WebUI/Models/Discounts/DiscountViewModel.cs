namespace Udemy.WebUI.Models.Discounts
{
    public class DiscountViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int Rate { get; set; }
        public string Code { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? AllowedCourseIds { get; set; }
    }
}
