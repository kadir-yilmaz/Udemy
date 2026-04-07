using Udemy.WebUI.Services.Abstract;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Udemy.WebUI.Settings;

namespace Udemy.WebUI.Services.Concrete
{
    public class ClientCredentialTokenService : IClientCredentialTokenService
    {
        private readonly ServiceApiSettings _serviceApiSettings;
        private readonly ClientSettings _clientSettings;
        private readonly HttpClient _httpClient;
        
        // Basit in-memory token cache
        private static readonly ConcurrentDictionary<string, (string Token, DateTime Expiry)> _tokenCache = new();

        public ClientCredentialTokenService(
            IOptions<ServiceApiSettings> serviceApiSettings, 
            IOptions<ClientSettings> clientSettings, 
            HttpClient httpClient)
        {
            _serviceApiSettings = serviceApiSettings.Value;
            _clientSettings = clientSettings.Value;
            _httpClient = httpClient;
        }

        public async Task<string> GetToken()
        {
            // Cache'den token kontrol et
            if (_tokenCache.TryGetValue("WebClientToken", out var cachedToken))
            {
                if (cachedToken.Expiry > DateTime.UtcNow.AddMinutes(1)) // 1 dakika margin
                {
                    return cachedToken.Token;
                }
            }

            var disco = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _serviceApiSettings.IdentityBaseUri,
                Policy = new DiscoveryPolicy { RequireHttps = false }
            });

            if (disco.IsError)
            {
                throw disco.Exception ?? new Exception($"Discovery error: {disco.Error}");
            }

            var clientCredentialTokenRequest = new ClientCredentialsTokenRequest
            {
                ClientId = _clientSettings.WebClient.ClientId,
                ClientSecret = _clientSettings.WebClient.ClientSecret,
                Address = disco.TokenEndpoint
            };

            var newToken = await _httpClient.RequestClientCredentialsTokenAsync(clientCredentialTokenRequest);

            if (newToken.IsError)
            {
                throw newToken.Exception;
            }

            // Token'ı cache'e kaydet
            var expiry = DateTime.UtcNow.AddSeconds(newToken.ExpiresIn);
            _tokenCache["WebClientToken"] = (newToken.AccessToken, expiry);

            return newToken.AccessToken;
        }
    }
}
