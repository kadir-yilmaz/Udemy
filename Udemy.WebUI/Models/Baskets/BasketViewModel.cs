using System;
using System.Collections.Generic;
using System.Linq;

namespace Udemy.WebUI.Models.Baskets
{
    public class BasketViewModel
    {
        public BasketViewModel()
        {
            _basketItems = new List<BasketItemViewModel>();
        }

        public string UserId { get; set; }
        public string? Email { get; set; }

        public string DiscountCode { get; set; }

        public int? DiscountRate { get; set; }
        public List<string>? AllowedCourseIds { get; set; }
        private List<BasketItemViewModel> _basketItems;

        public List<BasketItemViewModel> BasketItems
        {
            get
            {
                if (HasDiscount)
                {
                    _basketItems.ForEach(x =>
                    {
                        // Check if this item is eligible for discount
                        var isEligible = AllowedCourseIds == null || !AllowedCourseIds.Any() || AllowedCourseIds.Contains(x.CourseId);

                        if (isEligible)
                        {
                            var discountPrice = x.Price * ((decimal)DiscountRate.Value / 100);
                            x.AppliedDiscount(Math.Round(x.Price - discountPrice, 2));
                        }
                    });
                }
                return _basketItems;
            }
            set
            {
                _basketItems = value;
            }
        }

        public decimal TotalPrice
        {
            get => _basketItems.Sum(x => x.GetCurrentPrice);
        }

        public bool HasDiscount
        {
            get => !string.IsNullOrEmpty(DiscountCode) && DiscountRate.HasValue;
        }

        public void CancelDiscount()
        {
            DiscountCode = null;
            DiscountRate = null;
        }

        public void ApplyDiscount(string code, int rate)
        {
            DiscountCode = code;
            DiscountRate = rate;
        }
    }
}
