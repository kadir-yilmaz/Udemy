using Udemy.WebUI.Exceptions;
using Udemy.WebUI.Services.Abstract;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Udemy.WebUI.Handler
{
    public class ResourceOwnerPasswordTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdentityService _identityService;
        private readonly IClientCredentialTokenService _clientCredentialTokenService;
        private readonly ILogger<ResourceOwnerPasswordTokenHandler> _logger;

        // Token name constants
        private const string AccessToken = "access_token";
        private const string ExpiresAt = "expires_at";
        
        // Refresh buffer: refresh token if it will expire within this time
        private static readonly TimeSpan RefreshBuffer = TimeSpan.FromMinutes(5);

        public ResourceOwnerPasswordTokenHandler(
            IHttpContextAccessor httpContextAccessor, 
            IIdentityService identityService,
            IClientCredentialTokenService clientCredentialTokenService,
            ILogger<ResourceOwnerPasswordTokenHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _identityService = identityService;
            _clientCredentialTokenService = clientCredentialTokenService;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = await _httpContextAccessor.HttpContext!.GetTokenAsync(AccessToken);

            // Check if user has a token
            if (!string.IsNullOrEmpty(accessToken))
            {
                // Proactive refresh: Check if token is expiring soon
                if (await IsTokenExpiringSoon())
                {
                    _logger.LogInformation("[WebUI] Token expiring soon, refreshing proactively...");
                    Console.WriteLine("[WebUI] Handler: Token expiring soon -> Proactive Refresh");
                    
                    var tokenResponse = await _identityService.GetAccessTokenByRefreshToken();
                    if (tokenResponse != null)
                    {
                        accessToken = tokenResponse.AccessToken;
                        _logger.LogInformation("[WebUI] Token refreshed successfully");
                    }
                    else
                    {
                        _logger.LogWarning("[WebUI] Proactive refresh failed, using existing token");
                    }
                }

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            else
            {
                // No user token, use machine-to-machine token
                var clientCredentialToken = await _clientCredentialTokenService.GetToken();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clientCredentialToken);
            }

            var response = await base.SendAsync(request, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[WebUI] Request to {Uri} failed with {StatusCode}", request.RequestUri, response.StatusCode);
            }

            // Fallback: If we still get 401, try refresh one more time
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (!string.IsNullOrEmpty(accessToken))
                {
                    Console.WriteLine("[WebUI] Handler: 401 Unauthorized -> Attempting Fallback Refresh");
                    var tokenResponse = await _identityService.GetAccessTokenByRefreshToken();

                    if (tokenResponse != null)
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
                        response = await base.SendAsync(request, cancellationToken);
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Checks if the current access token is expiring within the refresh buffer period
        /// </summary>
        private async Task<bool> IsTokenExpiringSoon()
        {
            try
            {
                var expiresAtString = await _httpContextAccessor.HttpContext!.GetTokenAsync(ExpiresAt);
                
                if (string.IsNullOrEmpty(expiresAtString))
                {
                    // No expiration info, assume expired to trigger refresh
                    Console.WriteLine("[WebUI] Handler: No expires_at found, triggering refresh");
                    return true;
                }

                // Use DateTimeOffset for proper timezone handling
                if (DateTimeOffset.TryParse(expiresAtString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiresAt))
                {
                    var now = DateTimeOffset.Now;
                    var timeUntilExpiry = expiresAt - now;
                    
                    Console.WriteLine($"[WebUI] Handler: Token expires at {expiresAt:HH:mm:ss}, Now: {now:HH:mm:ss}, Remaining: {timeUntilExpiry.TotalMinutes:F1} min");
                    
                    if (timeUntilExpiry <= TimeSpan.Zero)
                    {
                        Console.WriteLine("[WebUI] Handler: Token EXPIRED! Triggering refresh");
                        return true;
                    }
                    
                    if (timeUntilExpiry <= RefreshBuffer)
                    {
                        Console.WriteLine("[WebUI] Handler: Token expiring soon, triggering proactive refresh");
                        return true;
                    }
                    
                    return false;
                }

                // Parsing failed, assume expired
                Console.WriteLine("[WebUI] Handler: Failed to parse expires_at, triggering refresh");
                return true;
            }
            catch (Exception ex)
            {
                // Any error, assume we need refresh
                Console.WriteLine($"[WebUI] Handler: Error checking expiry: {ex.Message}");
                return true;
            }
        }
    }
}
