namespace Udemy.Invoice.API.Settings
{
    /// <summary>
    /// Email gönderimi için SMTP ayarları
    /// appsettings.json'dan Options pattern ile okunur
    /// </summary>
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = null!;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = null!;
        public string SmtpPassword { get; set; } = null!;
        public string FromEmail { get; set; } = null!;
        public string FromName { get; set; } = null!;
        public bool EnableSsl { get; set; } = true;
    }
}
