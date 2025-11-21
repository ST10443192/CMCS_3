using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CMCS_3.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendClaimSubmittedEmailAsync(string lecturerEmail, int claimId);
        Task<bool> SendClaimApprovedEmailAsync(string lecturerEmail, int claimId);
        Task<bool> SendClaimRejectedEmailAsync(string lecturerEmail, int claimId, string reason);
        Task<bool> SendPaymentProcessedEmailAsync(string lecturerEmail, int claimId, decimal amount);
    }

    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailService()
        {
            // Configure SMTP settings (should come from configuration)
            _smtpServer = "smtp.example.com";
            _smtpPort = 587;
            _smtpUsername = "noreply@cmcs.com";
            _smtpPassword = "password";
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                    var message = new MailMessage
                    {
                        From = new MailAddress(_smtpUsername),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    message.To.Add(to);

                    await client.SendMailAsync(message);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Email send failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendClaimSubmittedEmailAsync(string lecturerEmail, int claimId)
        {
            string subject = "Claim Submitted Successfully";
            string body = $@"
                <h2>Claim Submitted</h2>
                <p>Your claim #{claimId} has been submitted successfully and is now pending approval.</p>
                <p>You will be notified once the claim has been reviewed.</p>
            ";

            return await SendEmailAsync(lecturerEmail, subject, body);
        }

        public async Task<bool> SendClaimApprovedEmailAsync(string lecturerEmail, int claimId)
        {
            string subject = "Claim Approved";
            string body = $@"
                <h2>Claim Approved</h2>
                <p>Your claim #{claimId} has been approved!</p>
                <p>Payment will be processed shortly.</p>
            ";

            return await SendEmailAsync(lecturerEmail, subject, body);
        }

        public async Task<bool> SendClaimRejectedEmailAsync(string lecturerEmail, int claimId, string reason)
        {
            string subject = "Claim Rejected";
            string body = $@"
                <h2>Claim Rejected</h2>
                <p>Unfortunately, your claim #{claimId} has been rejected.</p>
                <p><strong>Reason:</strong> {reason}</p>
                <p>Please contact your coordinator for more information.</p>
            ";


            return await SendEmailAsync(lecturerEmail, subject, body);
        }

        public async Task<bool> SendPaymentProcessedEmailAsync(string lecturerEmail, int claimId, decimal amount)
        {
            string subject = "Payment Processed";
            string body = $@"
                <h2>Payment Processed</h2>
                <p>Payment for claim #{claimId} has been processed successfully.</p>
                <p><strong>Amount:</strong> R{amount:N2}</p>
                <p>The payment should reflect in your account within 2-3 business days.</p>
            ";

            return await SendEmailAsync(lecturerEmail, subject, body);
        }
    }
}