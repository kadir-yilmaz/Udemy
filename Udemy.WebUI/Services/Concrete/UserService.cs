using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using Udemy.WebUI.Services.Abstract;
using Udemy.WebUI.Models;

namespace Udemy.WebUI.Services.Concrete
{
    public class UserService : IUserService
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserViewModel> GetUser()
        {
            return await _client.GetFromJsonAsync<UserViewModel>("/api/Users/GetUser");
        }

        public string GetUserId
        {
            get
            {
                // 1. Authenticated User (Login olmuş)
                var user = _httpContextAccessor.HttpContext?.User;
                if (user != null && user.Identity.IsAuthenticated)
                {
                    return user.FindFirst("sub")?.Value 
                         ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? "";
                }

                // 2. Guest User (Misafir) - Cookie Kontrolü
                // Eğer HttpContext yoksa boş dön
                if (_httpContextAccessor.HttpContext == null) return "";

                const string GuestCookieName = "udemy_guest_id";
                var requestCookies = _httpContextAccessor.HttpContext.Request.Cookies;
                var responseCookies = _httpContextAccessor.HttpContext.Response.Cookies;

                // Cookie var mı?
                if (requestCookies.TryGetValue(GuestCookieName, out var guestId))
                {
                    return guestId;
                }

                // Yoksa yeni oluştur
                var newGuestId = Guid.NewGuid().ToString();
                responseCookies.Append(GuestCookieName, newGuestId, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddDays(30), // 30 gün sakla
                    SameSite = SameSiteMode.Lax,
                    Secure = true 
                });

                return newGuestId;
            }
        }

        public string GetUserEmail
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user != null && user.Identity.IsAuthenticated)
                {
                    return user.FindFirst("email")?.Value 
                         ?? user.FindFirst(ClaimTypes.Email)?.Value 
                         ?? "";
                }

                return "Guest"; // Giriş yapmamışlar için etiket
            }
        }
    }
}
