using System.ComponentModel.DataAnnotations;

namespace Udemy.WebUI.Models.Orders
{
    public class CheckoutInfoInput
    {
        [Required(ErrorMessage = "Il alani zorunludur.")]
        [Display(Name = "Il")]
        public string Province { get; set; }

        [Required(ErrorMessage = "Ilce alani zorunludur.")]
        [Display(Name = "Ilce")]
        public string District { get; set; }

        [Required(ErrorMessage = "Cadde/Sokak alani zorunludur.")]
        [Display(Name = "Cadde")]
        public string Street { get; set; }

        [Required(ErrorMessage = "Posta kodu alani zorunludur.")]
        [Display(Name = "Posta Kodu")]
        public string ZipCode { get; set; }

        [Required(ErrorMessage = "Adres detayi alani zorunludur.")]
        [Display(Name = "Adres")]
        public string Line { get; set; }

        [Required(ErrorMessage = "Kart uzerindeki isim zorunludur.")]
        [Display(Name = "Kart Isim Soyisim")]
        public string CardName { get; set; }

        [Required(ErrorMessage = "Kart numarasi zorunludur.")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Kart numarasi 16 haneli ve sadece rakamlardan olusmalidir.")]
        [Display(Name = "Kart Numarasi")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Ay secimi zorunludur.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])$", ErrorMessage = "Gecerli bir ay seciniz.")]
        [Display(Name = "Son Kullanma Ayi")]
        public string ExpireMonth { get; set; }

        [Required(ErrorMessage = "Yil secimi zorunludur.")]
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Yil iki haneli olmalidir.")]
        [Display(Name = "Son Kullanma Yili")]
        public string ExpireYear { get; set; }

        [Required(ErrorMessage = "CVV zorunludur.")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV tam olarak 3 haneli olmalidir.")]
        [Display(Name = "CVV/CVC")]
        public string CVV { get; set; }

        [Display(Name = "Telefon Numarasi")]
        public string Phone { get; set; } = "+905555555555";
    }
}
