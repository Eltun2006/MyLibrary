using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;

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
                _logger.LogInformation("SendGrid email göndərilir: {Email}", toEmail);

                var apiKey = _config["SendGrid:ApiKey"];
                var fromEmail = _config["SendGrid:FromEmail"];
                var fromName = _config["SendGrid:FromName"] ?? "MyLibrary";

                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new Exception("SendGrid API Key tapılmadı! Render Environment Variables yoxlayın.");
                }

                _logger.LogInformation("SendGrid ApiKey: {KeyLength} simvol, From: {From}",
                    apiKey.Length, fromEmail);

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(fromEmail, fromName);
                var to = new EmailAddress(toEmail);

                var msg = MailHelper.CreateSingleEmail(
                    from,
                    to,
                    subject,
                    body,  // plain text version
                    body   // html version
                );

                _logger.LogInformation("SendGrid message yaradıldı, göndərilir...");

                var response = await client.SendEmailAsync(msg);

                _logger.LogInformation("SendGrid response: {StatusCode}", response.StatusCode);

                if (response.StatusCode != System.Net.HttpStatusCode.Accepted &&
                    response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var responseBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("SendGrid xətası: {Status}, Body: {Body}",
                        response.StatusCode, responseBody);
                    throw new Exception($"SendGrid xətası: {response.StatusCode}");
                }

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