using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net.Sockets;

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

                // SSL sertifikat yoxlanışını deaktiv et
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                // Timeout və CheckCertificateRevocation
                smtp.Timeout = 120000;
                smtp.CheckCertificateRevocation = false;

                var port = int.Parse(portStr);

                _logger.LogInformation("Qoşulur...");

                // STARTLS ilə qoşul
                await smtp.ConnectAsync(smtpServer, port, SecureSocketOptions.SslOnConnect);

                _logger.LogInformation("Port: {Port}", port);

                _logger.LogInformation("✓ Qoşuldu! Capabilities: {Capabilities}", smtp.Capabilities);

                _logger.LogInformation("Autentifikasiya...");
                await smtp.AuthenticateAsync(fromEmail, password);

                _logger.LogInformation("✓ Autentifikasiya OK!");

                _logger.LogInformation("Email göndərilir...");
                var result = await smtp.SendAsync(email);

                _logger.LogInformation("✓ Göndərildi! Response: {Response}", result);

                await smtp.DisconnectAsync(true);

                _logger.LogInformation("=== UĞURLU ===");
            }
            catch (AuthenticationException authEx)
            {
                _logger.LogError("❌ Autentifikasiya xətası: {Message}", authEx.Message);
                _logger.LogError("App Password düzgündürmü? {Pass}", password.Substring(0, 4) + "****");
                throw new Exception($"Gmail autentifikasiya xətası: {authEx.Message}", authEx);
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Xəta növü: {Type}", ex.GetType().Name);
                _logger.LogError("❌ Mesaj: {Message}", ex.Message);
                _logger.LogError("❌ InnerException: {Inner}", ex.InnerException?.Message);

                if (ex.StackTrace != null)
                {
                    var lines = ex.StackTrace.Split('\n').Take(5);
                    foreach (var line in lines)
                    {
                        _logger.LogError("  {Line}", line.Trim());
                    }
                }

                throw;
            }
        }


        public async Task<bool> IsValidEmailAsync(string email)
        {
            try 
            {
                var addr = new System.Net.Mail.MailAddress(email);

                return addr.Address == email;   
            }
            catch
            {
                return false;
            }

        }
    }
}