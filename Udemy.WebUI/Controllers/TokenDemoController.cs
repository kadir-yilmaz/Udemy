using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Udemy.WebUI.Services.Abstract;

namespace Udemy.WebUI.Controllers
{
    [Authorize]
    public class TokenDemoController : Controller
    {
        private readonly IClientCredentialTokenService _clientCredentialTokenService;
        private readonly IIdentityService _identityService;
        private readonly CookieAuthenticationOptions _cookieOptions;

        public TokenDemoController(
            IClientCredentialTokenService clientCredentialTokenService,
            IIdentityService identityService,
            IOptionsMonitor<CookieAuthenticationOptions> cookieOptionsMonitor)
        {
            _clientCredentialTokenService = clientCredentialTokenService;
            _identityService = identityService;
            _cookieOptions = cookieOptionsMonitor.Get(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> Index()
        {
            var model = new TokenDemoViewModel();

            // 1. User Token (Resource Owner Password - from Cookie)
            model.UserAccessToken = await HttpContext.GetTokenAsync("access_token");
            model.UserRefreshToken = await HttpContext.GetTokenAsync("refresh_token");
            model.UserTokenExpiresAt = await HttpContext.GetTokenAsync("expires_at");

            if (!string.IsNullOrEmpty(model.UserAccessToken))
            {
                model.UserTokenDecoded = DecodeJwt(model.UserAccessToken);
                var (expiresIn, totalMinutes, isExpired, isExpiringSoon) = CalculateExpiresIn(model.UserTokenExpiresAt);
                model.UserTokenExpiresIn = expiresIn;
                model.TotalMinutesRemaining = totalMinutes;
                model.IsExpired = isExpired;
                model.IsExpiringSoon = isExpiringSoon;
            }

            // Calculate Refresh Token Info
            var authResult2 = await HttpContext.AuthenticateAsync();
            if (!string.IsNullOrEmpty(model.UserRefreshToken) && authResult2.Succeeded && authResult2.Properties?.IssuedUtc != null)
            {
                var issuedAt = authResult2.Properties.IssuedUtc.Value;
                var expiresAt = issuedAt.AddDays(60); // Config'de 60 gün olarak ayarlandı
                var remaining = expiresAt - DateTimeOffset.UtcNow;

                model.RefreshTokenInfo = new RefreshTokenInfo
                {
                    TokenLength = model.UserRefreshToken.Length,
                    TokenType = "Opaque (Reference Token)",
                    UsagePolicy = "ReUse (Aynı token tekrar kullanılabilir)",
                    ExpirationPolicy = "Absolute (Sabit süre)",
                    ConfiguredLifetimeDays = 60,
                    IssuedAt = issuedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                    ExpiresAt = expiresAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                    RemainingDays = (int)remaining.TotalDays,
                    RemainingHours = remaining.Hours,
                    RemainingMinutes = remaining.Minutes,
                    IsExpired = remaining.TotalSeconds <= 0
                };
            }

            // 2. Client Credential Token (Machine-to-Machine)
            try
            {
                model.ClientCredentialToken = await _clientCredentialTokenService.GetToken();
                if (!string.IsNullOrEmpty(model.ClientCredentialToken))
                {
                    model.ClientTokenDecoded = DecodeJwt(model.ClientCredentialToken);
                }
            }
            catch (Exception ex)
            {
                model.ClientCredentialError = ex.Message;
            }

            // 3. Current User Claims
            model.UserClaims = User.Claims.Select(c => new ClaimInfo 
            { 
                Type = c.Type, 
                Value = c.Value 
            }).ToList();

            // 4. Current server time for reference
            model.ServerTimeUtc = DateTime.UtcNow;
            model.ServerTimeLocal = DateTime.Now;

            // 5. Cookie Information (from authentication ticket + IOptions)
            var authResult = await HttpContext.AuthenticateAsync();
            model.AuthCookieInfo = new CookieInfo
            {
                // Dynamic from IOptions<CookieAuthenticationOptions>
                Name = _cookieOptions.Cookie.Name ?? "(default)",
                IsHttpOnly = _cookieOptions.Cookie.HttpOnly,
                SlidingExpiration = _cookieOptions.SlidingExpiration,
                ConfiguredLifetimeDays = (int)_cookieOptions.ExpireTimeSpan.TotalDays,
                ConfiguredLifetimeHours = (int)_cookieOptions.ExpireTimeSpan.TotalHours % 24,
                
                // Runtime values
                Exists = authResult.Succeeded,
                IsSecure = HttpContext.Request.IsHttps,
                SameSite = _cookieOptions.Cookie.SameSite.ToString(),
                Path = _cookieOptions.Cookie.Path ?? "/",
                LoginPath = _cookieOptions.LoginPath.Value ?? "(not set)",
                AccessDeniedPath = _cookieOptions.AccessDeniedPath.Value ?? "(not set)"
            };

            if (authResult.Succeeded && authResult.Properties != null)
            {
                // IsPersistent from authentication properties
                model.AuthCookieInfo.IsPersistent = authResult.Properties.IsPersistent;
                model.AuthCookieInfo.AllowRefresh = authResult.Properties.AllowRefresh ?? _cookieOptions.SlidingExpiration;
                model.AuthCookieInfo.IssuedAt = authResult.Properties.IssuedUtc?.LocalDateTime.ToString("dd.MM.yyyy HH:mm:ss");
                
                if (authResult.Properties.ExpiresUtc != null)
                {
                    var expiresUtc = authResult.Properties.ExpiresUtc.Value;
                    model.AuthCookieInfo.ExpiresAt = expiresUtc.LocalDateTime.ToString("dd.MM.yyyy HH:mm:ss");
                    
                    var remaining = expiresUtc - DateTimeOffset.Now;
                    if (remaining.TotalDays >= 1)
                        model.AuthCookieInfo.ExpiresIn = $"{(int)remaining.TotalDays} gün {remaining.Hours} saat";
                    else if (remaining.TotalHours >= 1)
                        model.AuthCookieInfo.ExpiresIn = $"{(int)remaining.TotalHours} saat {remaining.Minutes} dakika";
                    else
                        model.AuthCookieInfo.ExpiresIn = $"{(int)remaining.TotalMinutes} dakika";
                }
            }

            // 6. List all tokens stored in cookie
            model.AuthCookieInfo.StoredTokens = new List<StoredTokenInfo>();
            
            var tokenNames = new[] { "access_token", "refresh_token", "expires_at", "id_token", "token_type" };
            foreach (var tokenName in tokenNames)
            {
                var tokenValue = await HttpContext.GetTokenAsync(tokenName);
                if (!string.IsNullOrEmpty(tokenValue))
                {
                    var isJwt = tokenValue.Contains(".") && tokenValue.Split('.').Length == 3;
                    model.AuthCookieInfo.StoredTokens.Add(new StoredTokenInfo
                    {
                        Name = tokenName,
                        Value = tokenValue.Length > 50 ? tokenValue.Substring(0, 50) + "..." : tokenValue,
                        FullValue = tokenValue,
                        Type = isJwt ? "JWT" : "Opaque/String",
                        Length = tokenValue.Length
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var result = await _identityService.GetAccessTokenByRefreshToken();
                if (result != null)
                {
                    TempData["RefreshSuccess"] = "Token başarıyla yenilendi!";
                }
                else
                {
                    TempData["RefreshError"] = "Token yenilenemedi. Lütfen tekrar giriş yapın.";
                }
            }
            catch (Exception ex)
            {
                TempData["RefreshError"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private JwtInfo DecodeJwt(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                return new JwtInfo
                {
                    Header = JsonSerializer.Serialize(jwt.Header, new JsonSerializerOptions { WriteIndented = true }),
                    Issuer = jwt.Issuer,
                    Audience = string.Join(", ", jwt.Audiences),
                    Subject = jwt.Subject,
                    IssuedAt = jwt.IssuedAt,
                    Expires = jwt.ValidTo,
                    Claims = jwt.Claims.Select(c => new ClaimInfo { Type = c.Type, Value = c.Value }).ToList()
                };
            }
            catch
            {
                return null;
            }
        }

        private (string display, double totalMinutes, bool isExpired, bool isExpiringSoon) CalculateExpiresIn(string expiresAt)
        {
            if (string.IsNullOrEmpty(expiresAt))
                return ("Bilinmiyor", 0, true, true);

            // Parse with timezone info (expires_at is stored in roundtrip format with offset)
            if (DateTimeOffset.TryParse(expiresAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiryOffset))
            {
                var now = DateTimeOffset.Now;
                var diff = expiryOffset - now;
                
                if (diff.TotalSeconds < 0)
                    return ("EXPIRED!", diff.TotalMinutes, true, true);
                
                var isExpiringSoon = diff.TotalMinutes <= 5;
                return ($"{(int)diff.TotalMinutes} dakika {diff.Seconds} saniye", diff.TotalMinutes, false, isExpiringSoon);
            }

            return ("Bilinmiyor", 0, true, true);
        }
    }

    // ViewModels
    public class TokenDemoViewModel
    {
        public string UserAccessToken { get; set; }
        public string UserRefreshToken { get; set; }
        public string UserTokenExpiresAt { get; set; }
        public string UserTokenExpiresIn { get; set; }
        public double TotalMinutesRemaining { get; set; }
        public bool IsExpired { get; set; }
        public bool IsExpiringSoon { get; set; }
        public JwtInfo UserTokenDecoded { get; set; }

        public string ClientCredentialToken { get; set; }
        public JwtInfo ClientTokenDecoded { get; set; }
        public string ClientCredentialError { get; set; }

        public List<ClaimInfo> UserClaims { get; set; } = new();
        
        public DateTime ServerTimeUtc { get; set; }
        public DateTime ServerTimeLocal { get; set; }

        // Cookie Information
        public CookieInfo AuthCookieInfo { get; set; }

        // Refresh Token Info
        public RefreshTokenInfo RefreshTokenInfo { get; set; }
    }

    public class RefreshTokenInfo
    {
        public int TokenLength { get; set; }
        public string TokenType { get; set; }
        public string UsagePolicy { get; set; }
        public string ExpirationPolicy { get; set; }
        public int ConfiguredLifetimeDays { get; set; }
        public string IssuedAt { get; set; }
        public string ExpiresAt { get; set; }
        public int RemainingDays { get; set; }
        public int RemainingHours { get; set; }
        public int RemainingMinutes { get; set; }
        public bool IsExpired { get; set; }
    }

    public class CookieInfo
    {
        // Configuration (from IOptions)
        public string Name { get; set; }
        public bool IsHttpOnly { get; set; }
        public bool SlidingExpiration { get; set; }
        public int ConfiguredLifetimeDays { get; set; }
        public int ConfiguredLifetimeHours { get; set; }
        public string SameSite { get; set; }
        public string Path { get; set; }
        public string LoginPath { get; set; }
        public string AccessDeniedPath { get; set; }
        
        // Runtime values
        public bool Exists { get; set; }
        public bool IsSecure { get; set; }
        public bool IsPersistent { get; set; }
        public bool AllowRefresh { get; set; }
        public string IssuedAt { get; set; }
        public string ExpiresAt { get; set; }
        public string ExpiresIn { get; set; }
        
        // All stored tokens in cookie
        public List<StoredTokenInfo> StoredTokens { get; set; } = new();
    }

    public class StoredTokenInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string FullValue { get; set; }
        public string Type { get; set; }
        public int Length { get; set; }
    }

    public class JwtInfo
    {
        public string Header { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Subject { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime Expires { get; set; }
        public List<ClaimInfo> Claims { get; set; } = new();
    }

    public class ClaimInfo
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}

