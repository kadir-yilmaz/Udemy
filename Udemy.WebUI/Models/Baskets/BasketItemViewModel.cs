namespace Udemy.WebUI.Models.Baskets
{
    public class BasketItemViewModel
    {
        public int Quantity { get; set; } = 1;

        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string PictureUrl { get; set; }

        public decimal Price { get; set; }

        private decimal? DiscountAppliedPrice;

        public decimal GetCurrentPrice
        {
            get => DiscountAppliedPrice != null ? DiscountAppliedPrice.Value : Price;
        }

        public bool HasDiscountApplied
        {
            get => DiscountAppliedPrice.HasValue;
        }

        public void AppliedDiscount(decimal discountPrice)
        {
            DiscountAppliedPrice = discountPrice;
        }
    }
}
