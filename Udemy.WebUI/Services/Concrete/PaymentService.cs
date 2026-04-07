using Udemy.WebUI.Models.FakePayments;
using Udemy.WebUI.Services.Abstract;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Udemy.WebUI.Services.Concrete
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;

        public PaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> ReceivePayment(PaymentInfoInput paymentInfoInput)
        {
            var response = await _httpClient.PostAsJsonAsync<PaymentInfoInput>("fakepayments", paymentInfoInput);

            return response.IsSuccessStatusCode;
        }
    }
}
