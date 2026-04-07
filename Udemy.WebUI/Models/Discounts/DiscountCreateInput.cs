using System.ComponentModel.DataAnnotations;

namespace Udemy.WebUI.Models.Discounts
{
    public class DiscountCreateInput
    {
        [Required(ErrorMessage = "Kupon kodu zorunludur")]
        [Display(Name = "Kupon Kodu")]
        public string Code { get; set; }

        [Required(ErrorMessage = "İndirim oranı zorunludur")]
        [Range(1, 100, ErrorMessage = "İndirim oranı 1-100 arasında olmalıdır")]
        [Display(Name = "İndirim Oranı (%)")]
        public int Rate { get; set; }

        [Required(ErrorMessage = "Lütfen en az bir kurs seçiniz.")]
        [Display(Name = "Geçerli Kurslar")]
        public List<string> AllowedCourseIds { get; set; }

        [Required(ErrorMessage = "Son kullanma tarihi zorunludur.")]
        [Display(Name = "Son Kullanma Tarihi")]
        public DateTime ExpirationDate { get; set; }

        public string? UserId { get; set; } // Arka planda dolacak
    }
}
