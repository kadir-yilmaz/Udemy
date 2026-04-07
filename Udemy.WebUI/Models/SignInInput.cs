using System.ComponentModel.DataAnnotations;

namespace Udemy.WebUI.Models
{
    public class SigninInput
    {
        [Required]
        [Display(Name = "Email adresiniz")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Şifreniz")]
        public string Password { get; set; }

        [Display(Name = "Beni hatırla")]
        public bool IsRemember { get; set; }
    }
}
