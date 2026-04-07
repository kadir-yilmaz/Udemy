using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Udemy.WebUI.Exceptions;
using Udemy.WebUI.Models;
using Udemy.WebUI.Services.Abstract;

namespace Udemy.WebUI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IIdentityService _identityService;
        private readonly IBasketService _basketService;

        public AuthController(IIdentityService identityService, IBasketService basketService)
        {
            _identityService = identityService;
            _basketService = basketService;
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SigninInput signinInput)
        {
            if (!ModelState.IsValid)
                return View();

            try
            {
                var userId = await _identityService.SignIn(signinInput);
                
                // Sepet Birleştirme (Merge)
                // Giriş yapan kullanıcının emailini gönderiyoruz ki etiketi güncellensin
                await _basketService.TransferBasket(userId, signinInput.Email);

                // Misafir Cookie'sini temizle
                Response.Cookies.Delete("udemy_guest_id");

                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            catch (IdentityException ex)
            {
                ex.Errors.ForEach(e => ModelState.AddModelError(string.Empty, e));
                TempData["ErrorMessage"] = string.Join(", ", ex.Errors);
                return View();
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Beklenmedik bir hata oluştu: " + ex.Message);
                TempData["ErrorMessage"] = "Sistem üzerinde bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.";
                return View();
            }
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpInput signUpInput)
        {
            Console.WriteLine($"[DEBUG-WEBUI] AuthController SignUp POST started for: {signUpInput.Email}");

            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = string.Join("<br/>", validationErrors);
                Console.WriteLine($"[DEBUG-WEBUI] ModelState is INVALID. Redirecting back with TempData error.");
                return View();
            }

            try
            {
                signUpInput.UserName = signUpInput.Email;
                await _identityService.SignUp(signUpInput);
                
                Console.WriteLine($"[DEBUG-WEBUI] SignUp SUCCESSFUL for: {signUpInput.Email}");
                TempData["SuccessMessage"] = "Kayıt başarılı! Şimdi giriş yapabilirsiniz.";
                return RedirectToAction(nameof(SignIn));
            }
            catch (IdentityException ex)
            {
                // Translate all common Identity errors to Turkish
                var translatedErrors = ex.Errors.Select(e => {
                    var lowerError = e.ToLower();
                    if (lowerError.Contains("already taken")) return $"'{signUpInput.Email}' e-posta adresi sistemde zaten kayıtlı.";
                    if (lowerError.Contains("non alphanumeric")) return "Şifre en az bir özel karakter (.,*,! vb.) içermelidir.";
                    if (lowerError.Contains("digit")) return "Şifre en az bir rakam ('0'-'9') içermelidir.";
                    if (lowerError.Contains("uppercase")) return "Şifre en az bir büyük harf ('A'-'Z') içermelidir.";
                    if (lowerError.Contains("lowercase")) return "Şifre en az bir küçük harf ('a'-'z') içermelidir.";
                    if (lowerError.Contains("too short") || lowerError.Contains("at least")) return "Şifre çok kısa veya eksik karakter içeriyor.";
                    return e; 
                }).ToList();

                Console.WriteLine($"[DEBUG-WEBUI] IdentityException caught: {string.Join(" | ", translatedErrors)}");

                translatedErrors.ForEach(e => ModelState.AddModelError(string.Empty, e));
                TempData["ErrorMessage"] = string.Join("<br/>", translatedErrors);
                return View();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"[DEBUG-WEBUI] UNEXPECTED EXCEPTION: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Sistemsel bir hata oluştu.");
                TempData["ErrorMessage"] = "Sistem üzerinde beklenmedik bir hata oluştu. Lütfen tekrar deneyiniz.";
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _identityService.RevokeRefreshToken();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
