using System.Text.Json;
using Udemy.WebUI.Models.FakePayments;
using Udemy.WebUI.Services.Abstract;

namespace Udemy.WebUI.Services.Concrete
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;

        public PaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> ReceivePayment(PaymentInfoInput paymentInfoInput)
        {
            var response = await _httpClient.PostAsJsonAsync("fakepayments", paymentInfoInput);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var rawContent = await response.Content.ReadAsStringAsync();

            try
            {
                var errorResponse = JsonSerializer.Deserialize<PaymentErrorResponse>(rawContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return (false, errorResponse?.Error ?? errorResponse?.Message ?? "Odeme basarisiz.");
            }
            catch
            {
                return (false, string.IsNullOrWhiteSpace(rawContent) ? "Odeme basarisiz." : rawContent);
            }
        }

        private sealed class PaymentErrorResponse
        {
            public string? Message { get; set; }
            public string? Error { get; set; }
        }
    }
}
