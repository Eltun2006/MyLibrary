using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net.Mail;
using System.Net.Sockets;
using System.Security.Authentication;

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
            var fromEmail = _config["EmailSettings:FromEmail"];
            var password = _config["EmailSettings:Password"];
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var portStr = _config["EmailSettings:Port"];

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(password))
            {
                throw new Exception("Email konfiqurasiyası tapılmadı!");
            }

            _logger.LogInformation("=== EMAIL GÖNDƏRİLİR ===");
            _logger.LogInformation("To: {Email}", toEmail);
            _logger.LogInformation("From: {From}", fromEmail);
            _logger.LogInformation("SMTP: {Server}:{Port}", smtpServer, portStr);

            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(fromEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;
                email.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

                using var smtp = new SmtpClient();

                // SSL sertifikat yoxlanışını deaktiv et (host-da self-signed ola bilər)
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                smtp.Timeout = 120000;
                smtp.CheckCertificateRevocation = false;

                var port = int.Parse(portStr);

                _logger.LogInformation("Qoşulur...");
                // ✅ Dəyişiklik burada: StartTLS və 587 port
                await smtp.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);

                _logger.LogInformation("✓ Qoşuldu! Capabilities: {Capabilities}", smtp.Capabilities);

                _logger.LogInformation("Autentifikasiya...");
                await smtp.AuthenticateAsync(fromEmail, password);
                _logger.LogInformation("✓ Autentifikasiya OK!");

                _logger.LogInformation("Email göndərilir...");
                var result = await smtp.SendAsync(email);
                _logger.LogInformation("✓ Göndərildi! Response: {Response}", result);

                await smtp.DisconnectAsync(true);
                _logger.LogInformation("=== EMAIL GÖNDƏRİLDİ (UĞURLU) ===");
            }
            catch (AuthenticationException authEx)
            {
                _logger.LogError("❌ Autentifikasiya xətası: {Message}", authEx.Message);
                throw new Exception($"Gmail autentifikasiya xətası: {authEx.Message}", authEx);
            }
            catch (SocketException sockEx)
            {
                _logger.LogError("❌ SMTP bağlantı xətası: {Message}", sockEx.Message);
                _logger.LogError("Render hostunda SMTP portu bağlı ola bilər.");
                throw new Exception($"SMTP bağlantı problemi: {sockEx.Message}", sockEx);
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Ümumi xəta: {Type} - {Message}", ex.GetType().Name, ex.Message);
                _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

        public async Task<bool> IsValidEmailAsync(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
