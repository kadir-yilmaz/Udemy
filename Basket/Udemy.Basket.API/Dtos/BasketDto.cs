namespace Udemy.Basket.API.Dtos
{
    public class BasketDto
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? DiscountCode { get; set; }
        public int? DiscountRate { get; set; }
        public List<BasketItemDto> BasketItems { get; set; } = new List<BasketItemDto>();
    }
}

