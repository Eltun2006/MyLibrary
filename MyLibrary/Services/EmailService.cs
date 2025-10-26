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
                _logger.LogInformation("Brevo email göndərilir: {Email}", toEmail);

                var fromEmail = _config["Brevo:FromEmail"];
                var username = _config["Brevo:Username"];
                var password = _config["Brevo:Password"];
                var smtpServer = _config["Brevo:SmtpServer"];
                var port = _config["Brevo:Port"];

                if (string.IsNullOrEmpty(password))
                {
                    throw new Exception("Brevo SMTP Key tapılmadı!");
                }

                _logger.LogInformation("Server: {Server}:{Port}, From: {From}",
                    smtpServer, port, fromEmail);

                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(fromEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

                using var smtp = new SmtpClient();
                smtp.Timeout = 30000;

                _logger.LogInformation("SMTP Connect...");

                await smtp.ConnectAsync(smtpServer, int.Parse(port), SecureSocketOptions.StartTls);

                _logger.LogInformation("SMTP Authenticate...");

                await smtp.AuthenticateAsync(username, password);

                _logger.LogInformation("Email göndərilir...");

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email uğurla göndərildi!");
            }
            catch (Exception ex)
            {
                _logger.LogError("Email xətası: {Message}, Type: {Type}",
                    ex.Message, ex.GetType().Name);
                throw new Exception($"Email göndərilə bilmədi: {ex.Message}");
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