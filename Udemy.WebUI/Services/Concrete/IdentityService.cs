using Udemy.WebUI.Models;
using Udemy.WebUI.Services.Abstract;
using Udemy.WebUI.Exceptions;
using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Udemy.WebUI.Settings;

namespace Udemy.WebUI.Services.Concrete
{
    public class IdentityService : IIdentityService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ClientSettings _clientSettings;
        private readonly ServiceApiSettings _serviceApiSettings;

        private const string AccessToken = "access_token";
        private const string RefreshToken = "refresh_token";
        private const string ExpiresAt = "expires_at";

        public IdentityService(
            HttpClient client, 
            IHttpContextAccessor httpContextAccessor, 
            IOptions<ClientSettings> clientSettings, 
            IOptions<ServiceApiSettings> serviceApiSettings)
        {
            _httpClient = client;
            _httpContextAccessor = httpContextAccessor;
            _clientSettings = clientSettings.Value;
            _serviceApiSettings = serviceApiSettings.Value;
        }

        public async Task<TokenResponse> GetAccessTokenByRefreshToken()
        {
            var disco = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _serviceApiSettings.IdentityBaseUri,
                Policy = new DiscoveryPolicy { RequireHttps = false }
            });

            if (disco.IsError)
                throw disco.Exception ?? new Exception($"Discovery endpoint error: {disco.Error}");

            var refreshToken = await _httpContextAccessor.HttpContext!.GetTokenAsync(RefreshToken);

            var token = await _httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                ClientId = _clientSettings.WebClientForUser.ClientId,
                ClientSecret = _clientSettings.WebClientForUser.ClientSecret,
                RefreshToken = refreshToken,
                Address = disco.TokenEndpoint
            });

            if (token.IsError)
                return null!;

            var authenticationTokens = new List<AuthenticationToken>
            {
                new() { Name = AccessToken, Value = token.AccessToken! },
                new() { Name = RefreshToken, Value = token.RefreshToken! },
                new() { Name = ExpiresAt, Value = DateTime.Now.AddSeconds(token.ExpiresIn).ToString("o", CultureInfo.InvariantCulture) }
            };

            var authResult = await _httpContextAccessor.HttpContext!.AuthenticateAsync();
            authResult.Properties!.StoreTokens(authenticationTokens);

            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                authResult.Principal!, 
                authResult.Properties);

            return token;
        }

        public async Task RevokeRefreshToken()
        {
            var disco = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _serviceApiSettings.IdentityBaseUri,
                Policy = new DiscoveryPolicy { RequireHttps = false }
            });

            if (disco.IsError)
                throw disco.Exception ?? new Exception($"Discovery endpoint error: {disco.Error}");

            var refreshToken = await _httpContextAccessor.HttpContext!.GetTokenAsync(RefreshToken);

            await _httpClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                ClientId = _clientSettings.WebClientForUser.ClientId,
                ClientSecret = _clientSettings.WebClientForUser.ClientSecret,
                Address = disco.RevocationEndpoint,
                Token = refreshToken,
                TokenTypeHint = "refresh_token"
            });
        }

        public async Task<string> SignIn(SigninInput signinInput)
        {
            var disco = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _serviceApiSettings.IdentityBaseUri,
                Policy = new DiscoveryPolicy { RequireHttps = false }
            });

            if (disco.IsError)
                throw disco.Exception ?? new Exception($"Discovery endpoint error: {disco.Error}");

            var token = await _httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                ClientId = _clientSettings.WebClientForUser.ClientId,
                ClientSecret = _clientSettings.WebClientForUser.ClientSecret,
                UserName = signinInput.Email,
                Password = signinInput.Password,
                Address = disco.TokenEndpoint,
                Scope = "openid profile email offline_access basket_fullpermission order_fullpermission gateway_fullpermission catalog_fullpermission photo_stock_fullpermission IdentityServerApi roles"
            });

            if (token.IsError)
            {
                var content = await token.HttpResponse.Content.ReadAsStringAsync();
                var errorDto = JsonSerializer.Deserialize<ErrorDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                throw new IdentityException(errorDto?.Errors ?? new List<string> { "Giriş başarısız" });
            }

            var userInfo = await _httpClient.GetUserInfoAsync(new UserInfoRequest
            {
                Token = token.AccessToken,
                Address = disco.UserInfoEndpoint
            });

            if (userInfo.IsError)
                throw userInfo.Exception ?? new Exception($"UserInfo endpoint error: {userInfo.Error}");

            var claimsIdentity = new ClaimsIdentity(userInfo.Claims, CookieAuthenticationDefaults.AuthenticationScheme, "name", "role");
            
            // Explicitly ensure NameIdentifier is present if 'sub' exists
            var subClaim = userInfo.Claims.FirstOrDefault(x => x.Type == "sub");
            if (subClaim != null)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
            }
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProps = new AuthenticationProperties { IsPersistent = signinInput.IsRemember };
            authProps.StoreTokens(new List<AuthenticationToken>
            {
                new() { Name = AccessToken, Value = token.AccessToken! },
                new() { Name = RefreshToken, Value = token.RefreshToken! },
                new() { Name = ExpiresAt, Value = DateTime.Now.AddSeconds(token.ExpiresIn).ToString("o", CultureInfo.InvariantCulture) }
            });

            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                claimsPrincipal, 
                authProps);
            
            return subClaim?.Value ?? "";
        }

        public async Task SignUp(SignUpInput signUpInput)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    signUpInput.UserName,
                    signUpInput.Email,
                    signUpInput.Password,
                    signUpInput.Name,
                    signUpInput.Surname
                }),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                $"{_serviceApiSettings.IdentityBaseUri}/api/Users/SignUp", 
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                // CRITICAL DEBUG LOGS
                Console.WriteLine($"[DEBUG-WEBUI] IdentityServer SignUp FAILED. Status: {response.StatusCode}");
                Console.WriteLine($"[DEBUG-WEBUI] IdentityServer SignUp RAW Response: {errorContent}");

                try
                {
                    var errorDto = JsonSerializer.Deserialize<ErrorDto>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var errors = errorDto?.Errors ?? new List<string> { $"Bilinmeyen bir hata oluştu (Status: {response.StatusCode})" };
                    
                    // Log the extracted errors
                    Console.WriteLine($"[DEBUG-WEBUI] Extracted Errors: {string.Join(" | ", errors)}");
                    
                    throw new IdentityException(errors);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[DEBUG-WEBUI] JSON Deserialization FAILED: {ex.Message}");
                    throw new IdentityException(new List<string> { $"Sunucudan geçersiz yanıt alındı ({response.StatusCode}). Detay: {errorContent}" });
                }
            }
        }
    }
}
