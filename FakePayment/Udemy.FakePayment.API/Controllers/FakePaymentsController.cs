using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Udemy.FakePayment.API.Models;
using Udemy.FakePayment.API.Settings;
using Udemy.Shared.Events;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using System.Globalization;

namespace Udemy.FakePayment.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class FakePaymentsController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly Iyzipay.Options _iyzipayOptions;

        public FakePaymentsController(
            IPublishEndpoint publishEndpoint,
            IOptions<IyzipaySettings> iyzipaySettings)
        {
            _publishEndpoint = publishEndpoint;
            
            var settings = iyzipaySettings.Value;
            _iyzipayOptions = new Iyzipay.Options
            {
                ApiKey = settings.ApiKey,
                SecretKey = settings.SecretKey,
                BaseUrl = settings.BaseUrl
            };
        }

        [HttpPost]
        public async Task<IActionResult> ReceivePayment(PaymentDto paymentDto)
        {
            Console.WriteLine("[FakePaymentsController] POST ReceivePayment called.");
            Console.WriteLine($"[FakePaymentsController] Card: {paymentDto.CardNumber}, Amount: {paymentDto.TotalPrice}");

            if (string.IsNullOrWhiteSpace(_iyzipayOptions.ApiKey) || string.IsNullOrWhiteSpace(_iyzipayOptions.SecretKey))
            {
                Console.WriteLine("[FakePaymentsController] ℹ️ Iyzipay keys are empty. Using local simulation mode.");

                await _publishEndpoint.Publish(new PaymentCompleted { OrderId = paymentDto.OrderId });
                await PublishInvoiceEvent(paymentDto, paymentDto.TotalPrice);

                return Ok(new
                {
                    Success = true,
                    Message = "Simulated payment succeeded",
                    PaymentId = $"SIM-{paymentDto.OrderId}"
                });
            }
            
            // 💰 0 TL kontrolü - ücretsiz satın alma (iyzico 0 TL kabul etmiyor)
            if (paymentDto.TotalPrice <= 0)
            {
                Console.WriteLine("[FakePaymentsController] ✅ FREE PURCHASE - Skipping iyzico");
                
                // PaymentCompleted event publish et - Order status güncellenecek
                await _publishEndpoint.Publish(new PaymentCompleted { OrderId = paymentDto.OrderId });
                Console.WriteLine($"[FakePaymentsController] 📤 PaymentCompleted event published for FREE OrderId: {paymentDto.OrderId}");
                
                // Fatura event'i de gönder
                await PublishInvoiceEvent(paymentDto, 0);
                
                return Ok(new { 
                    Success = true, 
                    Message = "Ücretsiz satın alma başarılı",
                    PaymentId = "FREE-" + paymentDto.OrderId
                });
            }
            
            // iyzico ödeme isteği oluştur
            var paymentResult = await ProcessIyzipayPayment(paymentDto);
            
            if (paymentResult.Status != "success")
            {
                Console.WriteLine($"[FakePaymentsController] ❌ Payment FAILED: {paymentResult.ErrorMessage}");
                
                // 📤 PaymentFailed event publish et - Order status Failed olacak
                await _publishEndpoint.Publish(new PaymentFailed 
                { 
                    OrderId = paymentDto.OrderId, 
                    Reason = paymentResult.ErrorMessage ?? "Ödeme başarısız" 
                });
                Console.WriteLine($"[FakePaymentsController] 📤 PaymentFailed event published for OrderId: {paymentDto.OrderId}");
                
                return BadRequest(new { 
                    Success = false, 
                    Message = "Ödeme başarısız.", 
                    Error = paymentResult.ErrorMessage,
                    ErrorCode = paymentResult.ErrorCode
                });
            }
            
            // 📤 PaymentCompleted event publish et - Order status Completed olacak
            await _publishEndpoint.Publish(new PaymentCompleted { OrderId = paymentDto.OrderId });
            Console.WriteLine($"[FakePaymentsController] 📤 PaymentCompleted event published for OrderId: {paymentDto.OrderId}");
            
            // 📧 Invoice event'i publish et
            await PublishInvoiceEvent(paymentDto, paymentDto.TotalPrice);
            
            Console.WriteLine($"[FakePaymentsController] ✅ Payment SUCCESS - PaymentId: {paymentResult.PaymentId}");
            return Ok(new { 
                Success = true, 
                Message = "Ödeme başarılı",
                PaymentId = paymentResult.PaymentId 
            });
        }
        
        private async Task PublishInvoiceEvent(PaymentDto paymentDto, decimal totalPrice)
        {
            var invoiceEvent = new InvoiceRequested
            {
                OrderId = paymentDto.OrderId,
                CustomerName = $"{paymentDto.CustomerName} {paymentDto.CustomerSurname}",
                CustomerEmail = paymentDto.CustomerEmail,
                TotalPrice = totalPrice,
                OrderDate = DateTime.Now,
                Items = paymentDto.Items.Select(x => new InvoiceItemMessage
                {
                    ProductName = x.ProductName,
                    Price = x.Price
                }).ToList()
            };
            
            await _publishEndpoint.Publish(invoiceEvent);
            Console.WriteLine($"[FakePaymentsController] 📧 InvoiceRequested event published for OrderId: {paymentDto.OrderId}");
        }
        
        private async Task<Payment> ProcessIyzipayPayment(PaymentDto dto)
        {
            var request = new CreatePaymentRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = $"Udemy-{dto.OrderId}",
                Price = dto.TotalPrice.ToString("F2", CultureInfo.InvariantCulture),
                PaidPrice = dto.TotalPrice.ToString("F2", CultureInfo.InvariantCulture),
                Currency = Currency.TRY.ToString(),
                Installment = 1,
                BasketId = $"B{dto.OrderId}",
                PaymentChannel = PaymentChannel.WEB.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                PaymentCard = new PaymentCard
                {
                    CardHolderName = dto.CardName,
                    CardNumber = dto.CardNumber,
                    ExpireMonth = dto.ExpireMonth,
                    ExpireYear = dto.ExpireYear,
                    Cvc = dto.CVV,
                    RegisterCard = 0
                },
                Buyer = new Buyer
                {
                    Id = $"BY{dto.OrderId}",
                    Name = dto.CustomerName,
                    Surname = dto.CustomerSurname,
                    GsmNumber = dto.CustomerPhone,
                    Email = dto.CustomerEmail,
                    IdentityNumber = dto.CustomerIdentityNumber,
                    RegistrationAddress = dto.AddressDetail,
                    City = dto.City,
                    Country = dto.Country,
                    ZipCode = dto.ZipCode,
                    Ip = dto.CustomerIp
                },
                ShippingAddress = new Iyzipay.Model.Address
                {
                    ContactName = $"{dto.CustomerName} {dto.CustomerSurname}",
                    City = dto.City,
                    Country = dto.Country,
                    Description = dto.AddressDetail,
                    ZipCode = dto.ZipCode
                },
                BillingAddress = new Iyzipay.Model.Address
                {
                    ContactName = $"{dto.CustomerName} {dto.CustomerSurname}",
                    City = dto.City,
                    Country = dto.Country,
                    Description = dto.AddressDetail,
                    ZipCode = dto.ZipCode
                },
                BasketItems = dto.Items.Select((item, i) => new BasketItem
                {
                    Id = $"BI{i + 1}",
                    Name = item.ProductName,
                    Category1 = "General",
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = item.Price.ToString("F2", CultureInfo.InvariantCulture)
                }).ToList()
            };

            return await Task.Run(() => Payment.Create(request, _iyzipayOptions));
        }
    }
}


