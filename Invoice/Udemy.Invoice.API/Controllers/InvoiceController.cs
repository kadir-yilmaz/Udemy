using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Udemy.Invoice.API.Models;
using Udemy.Invoice.API.Services;

namespace Udemy.Invoice.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public InvoiceController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        /// <summary>
        /// Fatura emaili gönderir
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendInvoice([FromBody] InvoiceData invoice)
        {
            Console.WriteLine($"[InvoiceController] SendInvoice called for OrderId: {invoice.OrderId}");
            
            var result = await _emailService.SendInvoiceEmailAsync(invoice);
            
            if (result)
            {
                return Ok(new { Success = true, Message = "Fatura emaili gönderildi" });
            }
            
            return BadRequest(new { Success = false, Message = "Email gönderilemedi" });
        }
        
        /// <summary>
        /// Test endpoint - Email ayarlarını kontrol eder
        /// </summary>
        [HttpGet("test")]
        public async Task<IActionResult> TestEmail()
        {
            var testInvoice = new InvoiceData
            {
                OrderId = 999,
                CustomerName = "Test Kullanıcı",
                CustomerEmail = "kadiryilmaz19821@gmail.com",
                OrderDate = DateTime.Now,
                TotalPrice = 199.99m,
                Items = new List<InvoiceItem>
                {
                    new() { ProductName = "Python Full Stack", Price = 99.99m },
                    new() { ProductName = "Java Masterclass", Price = 100.00m }
                }
            };
            
            var result = await _emailService.SendInvoiceEmailAsync(testInvoice);
            
            if (result)
            {
                return Ok("✅ Test emaili gönderildi! Inbox'ını kontrol et.");
            }
            
            return BadRequest("❌ Email gönderilemedi. SMTP ayarlarını kontrol et.");
        }
    }
}
