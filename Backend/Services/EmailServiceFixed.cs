using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class EmailServiceFixed : IEmailService
    {
        private readonly string? _smtpHost;
        private readonly int _smtpPort;
        private readonly string? _smtpUsername;
        private readonly string? _smtpPassword;
        private readonly string? _fromEmail;

        public EmailServiceFixed()
        {
            _smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
            _smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
            _smtpUsername = Environment.GetEnvironmentVariable("SMTP_USER");
            _smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASS");
            _fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL") ?? _smtpUsername;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
            {
                Console.WriteLine($"Password reset token for {toEmail}: {resetToken}");
                Console.WriteLine("SMTP not configured. Email not sent.");
                return;
            }

            var resetUrl = $"{Environment.GetEnvironmentVariable("FRONTEND_BASE") ?? "http://localhost:5173"}/reset?token={resetToken}";
            
            var subject = "Password Reset Request - MediSync";
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>You have requested to reset your password for your MediSync account.</p>
                    <p>Please click the link below to reset your password:</p>
                    <p><a href='{resetUrl}'>Reset Password</a></p>
                    <p>Or copy and paste this token: <strong>{resetToken}</strong></p>
                    <p>This link will expire in 1 hour.</p>
                    <p>If you did not request this password reset, please ignore this email.</p>
                </body>
                </html>";

            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            var message = new MailMessage(_fromEmail!, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }
    }
}