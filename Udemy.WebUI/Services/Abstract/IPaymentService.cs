using Udemy.WebUI.Models.FakePayments;
using System.Threading.Tasks;

namespace Udemy.WebUI.Services.Abstract
{
    public interface IPaymentService
    {
        Task<bool> ReceivePayment(PaymentInfoInput paymentInfoInput);
    }
}
