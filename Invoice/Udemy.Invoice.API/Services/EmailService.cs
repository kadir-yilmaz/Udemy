using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Udemy.Invoice.API.Models;
using Udemy.Invoice.API.Settings;

namespace Udemy.Invoice.API.Services
{
    public interface IEmailService
    {
        Task<bool> SendInvoiceEmailAsync(InvoiceData invoice);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task<bool> SendInvoiceEmailAsync(InvoiceData invoice)
        {
            try
            {
                Console.WriteLine(
                    $"[EmailService] SMTP config Host={_emailSettings.SmtpHost}, Port={_emailSettings.SmtpPort}, " +
                    $"EnableSsl={_emailSettings.EnableSsl}, Username={_emailSettings.SmtpUsername}, FromEmail={_emailSettings.FromEmail}, " +
                    $"PasswordLength={_emailSettings.SmtpPassword?.Length ?? 0}");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(MailboxAddress.Parse(invoice.CustomerEmail));
                message.Subject = $"Udemy - Siparis Onayi #{invoice.OrderId}";
                message.Body = new BodyBuilder
                {
                    HtmlBody = GenerateReceiptHtml(invoice)
                }.ToMessageBody();

                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(
                    _emailSettings.SmtpHost,
                    _emailSettings.SmtpPort,
                    _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                await smtpClient.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);

                Console.WriteLine($"[EmailService] Email sent successfully to: {invoice.CustomerEmail}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Email sending failed. Type={ex.GetType().FullName}, Message={ex.Message}");
                Console.WriteLine($"[EmailService] Exception detail: {ex}");
                return false;
            }
        }

        private string GenerateReceiptHtml(InvoiceData invoice)
        {
            var itemsHtml = string.Join("", invoice.Items.Select(item => $@"
                <tr>
                    <td style=""padding: 12px; border-bottom: 1px solid #e0e0e0; color: #333;"">{item.ProductName}</td>
                    <td style=""padding: 12px; border-bottom: 1px solid #e0e0e0; text-align: right; color: #333;"">{item.Price:C}</td>
                </tr>"));

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>Siparis Onayi</title>
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff;"">
        <div style=""background-color: #1c1c1c; padding: 30px; text-align: center;"">
            <h1 style=""color: #a855f7; margin: 0; font-size: 28px;"">Udemy</h1>
        </div>

        <div style=""padding: 40px; text-align: center;"">
            <h1 style=""color: #1c1c1c; margin: 0 0 10px 0; font-size: 32px;"">Tesekkurler.</h1>
            <p style=""color: #666; margin: 0; font-size: 16px;"">Merhaba {invoice.CustomerName}!</p>
            <p style=""color: #666; margin: 5px 0 0 0; font-size: 16px;"">Satin alimin icin tesekkur ederiz!</p>
        </div>

        <div style=""padding: 20px 40px; text-align: center;"">
            <p style=""color: #666; margin: 0; font-size: 14px; font-weight: bold;"">FATURA KIMLIGI:</p>
            <h2 style=""color: #1c1c1c; margin: 10px 0; font-size: 28px; font-weight: bold;"">INV-{invoice.InvoiceNumber}</h2>
        </div>

        <div style=""padding: 20px 40px; border-top: 1px solid #e0e0e0;"">
            <p style=""color: #666; margin: 0 0 15px 0; font-size: 12px; font-weight: bold; text-transform: uppercase;"">SIPARIS BILGILERI:</p>
            <table style=""width: 100%; font-size: 14px;"">
                <tr>
                    <td style=""color: #666; padding: 5px 0;"">Siparis ID:</td>
                    <td style=""color: #333; padding: 5px 0;"">{invoice.OrderId}</td>
                </tr>
                <tr>
                    <td style=""color: #666; padding: 5px 0;"">Tarih:</td>
                    <td style=""color: #333; padding: 5px 0;"">{invoice.OrderDate:dd MMMM yyyy HH:mm}</td>
                </tr>
                <tr>
                    <td style=""color: #666; padding: 5px 0;"">Email:</td>
                    <td style=""color: #333; padding: 5px 0;"">{invoice.CustomerEmail}</td>
                </tr>
            </table>
        </div>

        <div style=""padding: 20px 40px;"">
            <p style=""color: #666; margin: 0 0 15px 0; font-size: 12px; font-weight: bold; text-transform: uppercase;"">SATIN ALINAN KURSLAR:</p>
            <table style=""width: 100%; border-collapse: collapse;"">
                <thead>
                    <tr style=""background-color: #f9f9f9;"">
                        <th style=""padding: 12px; text-align: left; color: #666; font-weight: 600; font-size: 12px; text-transform: uppercase;"">Kurs</th>
                        <th style=""padding: 12px; text-align: right; color: #666; font-weight: 600; font-size: 12px; text-transform: uppercase;"">Fiyat</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsHtml}
                </tbody>
                <tfoot>
                    <tr style=""background-color: #f9f9f9;"">
                        <td style=""padding: 15px 12px; font-weight: bold; color: #1c1c1c;"">TOPLAM</td>
                        <td style=""padding: 15px 12px; text-align: right; font-weight: bold; color: #a855f7; font-size: 18px;"">{invoice.TotalPrice:C}</td>
                    </tr>
                </tfoot>
            </table>
        </div>

        <div style=""background-color: #1c1c1c; padding: 30px; text-align: center;"">
            <p style=""color: #999; margin: 0; font-size: 12px;"">Bu email otomatik olarak gonderilmistir.</p>
            <p style=""color: #999; margin: 5px 0 0 0; font-size: 12px;"">© 2026 Udemy Clone - Tum haklari saklidir.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
