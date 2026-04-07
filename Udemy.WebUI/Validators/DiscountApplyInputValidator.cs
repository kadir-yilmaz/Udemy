using FluentValidation;
using Udemy.WebUI.Models.Discounts;

namespace Udemy.WebUI.Validators
{
    public class DiscountApplyInputValidator : AbstractValidator<DiscountApplyInput>
    {
        public DiscountApplyInputValidator()
        {
            RuleFor(x => x.Code).NotEmpty().WithMessage("İndirim kupon alanı boş olamaz");
        }
    }
}
