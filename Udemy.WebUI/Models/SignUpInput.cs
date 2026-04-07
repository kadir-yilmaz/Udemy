using System.ComponentModel.DataAnnotations;

namespace Udemy.WebUI.Models
{
    public class SignUpInput
    {
        [Required(ErrorMessage = "Ad zorunludur")]
        [Display(Name = "Ad")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Soyad zorunludur")]
        [Display(Name = "Soyad")]
        public string Surname { get; set; } = null!;
        [Display(Name = "Kullanıcı Adı")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Email zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Şifre zorunludur")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = null!;
    }
}
