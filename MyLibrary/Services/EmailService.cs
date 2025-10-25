using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MyLibrary.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task<bool> IsValidEmailAsync(string email);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                _logger.LogInformation("Email göndərilir: {Email}", toEmail);

                // Konfiqurasiya yoxlanışı
                var fromEmail = _config["EmailSettings:FromEmail"];
                var password = _config["EmailSettings:Password"];
                var smtpServer = _config["EmailSettings:SmtpServer"];
                var port = _config["EmailSettings:Port"];

                if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(password))
                {
                    throw new Exception("Email konfiqurasiyası tapılmadı!");
                }

                _logger.LogInformation("SMTP Server: {Server}:{Port}, From: {From}",
                    smtpServer, port, fromEmail);

                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(fromEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = body
                };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();

                // Timeout əlavə et
                smtp.Timeout = 30000; // 30 saniyə

                await smtp.ConnectAsync(
                    smtpServer,
                    int.Parse(port),
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(fromEmail, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email uğurla göndərildi: {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError("Email xətası: {Message}, StackTrace: {Stack}",
                    ex.Message, ex.StackTrace);
                throw new Exception($"Email göndərilə bilmədi: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsValidEmailAsync(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && email.EndsWith("@gmail.com");
            }
            catch
            {
                return false;
            }
        }
    }
}