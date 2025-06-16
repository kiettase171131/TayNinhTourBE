using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using TayNinhTourApi.BusinessLogicLayer.Common;
using static System.Net.WebRequestMethods;

namespace TayNinhTourApi.BusinessLogicLayer.Utilities
{
    public class EmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendOtpRegisterAsync(string toEmail, string otp)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Tay Ninh Tour: Your Registration OTP Code";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h2>Welcome to Tay Ninh Tour!</h2>
                    <p>Dear Customer,</p>
                    <p>Thank you for choosing us. To complete your registration, please use the one-time password (OTP) below:</p>
                    <p style=""font-size: 18px; font-weight: bold; color: #2c3e50;"">{otp}</p>
                    <p>This OTP is valid for <strong>5 minutes</strong>. Please enter it on the registration page to verify your account.</p>
                    <p>We’re excited to have you with us!</p>
                    <p>Best regards,<br>Tay Ninh Tour Team</p>"
            };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendOtpResetPasswordAsync(string toEmail, string otp)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Tay Ninh Tour: Your Reset Password OTP Code";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h2>Welcome to Tay Ninh Tour!</h2>
                    <p>Dear Customer,</p>
                    <p>Thank you for choosing us. To complete your Reset Password, please use the one-time password (OTP) below:</p>
                    <p style=""font-size: 18px; font-weight: bold; color: #2c3e50;"">{otp}</p>
                    <p>This OTP is valid for <strong>5 minutes</strong>. Please enter it on the Reset Password page to reset your password.</p>
                    <p>We’re excited to have you with us!</p>
                    <p>Best regards,<br>Tay Ninh Tour Team</p>"
            };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
        public async Task SendApprovalNotificationAsync(string toEmail, string userName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(userName, toEmail));
            message.Subject = "Tay Ninh Tour: Your Tour Guide Application Has Been Approved";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <h2>Hello {userName},</h2>
            <p>We are pleased to inform you that your Tour Guide application has been <strong>approved</strong>!</p>
            <p>You now have access to all the Tour Guide features on the Tay Ninh Tour platform.</p>
            <p>We wish you exciting and rewarding journeys ahead!</p>
            <br/>
            <p>Best regards,</p>
            <p>The Tay Ninh Tour Team</p>"
            };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // Connect to SMTP server using StartTLS
                await client.ConnectAsync(
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort,
                    MailKit.Security.SecureSocketOptions.StartTls
                );

                // Authenticate if required
                await client.AuthenticateAsync(
                    _emailSettings.Username,
                    _emailSettings.Password
                );

                // Send the email
                await client.SendAsync(message);

                // Disconnect
                await client.DisconnectAsync(true);
            }
        }
        public async Task SendRejectionNotificationAsync(string toEmail, string userName, string reason)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(userName, toEmail));
            message.Subject = "Tay Ninh Tour: Your Tour Guide Application Has Been Rejected";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <h2>Hello {userName},</h2>
            <p>Thank you for your interest in becoming a Tour Guide with Tay Ninh Tour.</p>
            <p>After careful review, we regret to inform you that your application has been <strong>rejected</strong> for the following reason:</p>
            <blockquote style=""margin: 10px 0; padding: 10px; border-left: 4px solid #ccc;"">
                {reason}
            </blockquote>
            <p>We encourage you to apply again in the future and appreciate the time you took to submit your application.</p>
            <br/>
            <p>Best regards,</p>
            <p>The Tay Ninh Tour Team</p>"
            };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort,
                    MailKit.Security.SecureSocketOptions.StartTls
                );
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendShopApprovalNotificationAsync(string toEmail, string userName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(userName, toEmail));
            message.Subject = "Tay Ninh Tour: Your Specialty Shop Application Has Been Approved";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
      <h2>Hello {userName},</h2>
      <p>We are pleased to inform you that your Specialty Shop application has been <strong>approved</strong>!</p>
      <p>You now have access to all the Specialty Shop features on the Tay Ninh Tour platform.</p>
      <p>We wish you exciting and rewarding journeys ahead!</p>
      <br/>
      <p>Best regards,</p>
      <p>The Tay Ninh Tour Team</p>"
            };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // Connect to SMTP server using StartTLS
                await client.ConnectAsync(
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort,
                    MailKit.Security.SecureSocketOptions.StartTls
                );

                // Authenticate if required
                await client.AuthenticateAsync(
                    _emailSettings.Username,
                    _emailSettings.Password
                );

                // Send the email
                await client.SendAsync(message);

                // Disconnect
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendShopRejectionNotificationAsync(string toEmail, string userName, string reason)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(userName, toEmail));
            message.Subject = "Tay Ninh Tour: Your Specialty Shop Application Has Been Rejected";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
     <h2>Hello {userName},</h2>
     <p>Thank you for your interest in becoming a Specialty Shop with Tay Ninh Tour.</p>
     <p>After careful review, we regret to inform you that your application has been <strong>rejected</strong> for the following reason:</p>
     <blockquote style=""margin: 10px 0; padding: 10px; border-left: 4px solid #ccc;"">
         {reason}
     </blockquote>
     <p>We encourage you to apply again in the future and appreciate the time you took to submit your application.</p>
     <br/>
     <p>Best regards,</p>
     <p>The Tay Ninh Tour Team</p>"
            };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort,
                    MailKit.Security.SecureSocketOptions.StartTls
                );
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        /// <summary>
        /// Send email confirmation khi user nộp đơn SpecialtyShop
        /// </summary>
        public async Task SendSpecialtyShopApplicationSubmittedAsync(string toEmail, string userName, string shopName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(userName, toEmail));
            message.Subject = "Tay Ninh Tour: Specialty Shop Application Received";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <h2>Hello {userName},</h2>
            <p>Thank you for submitting your specialty shop application for <strong>{shopName}</strong>!</p>
            <p>We have received your application and it is currently under review.</p>
            <p><strong>What happens next:</strong></p>
            <ul>
                <li>Our team will review your application within 3-5 business days</li>
                <li>We will verify your business license and shop information</li>
                <li>You will receive an email notification once the review is complete</li>
            </ul>
            <p>If you have any questions, please contact our support team.</p>
            <br/>
            <p>Best regards,</p>
            <p>The Tay Ninh Tour Team</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, true);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        /// <summary>
        /// Send email khi SpecialtyShop application được approve
        /// </summary>
        public async Task SendSpecialtyShopApprovalNotificationAsync(string toEmail, string userName, string shopName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(userName, toEmail));
            message.Subject = "Congratulations! Your Specialty Shop Application Has Been Approved";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <h2>Congratulations {userName}!</h2>
            <p>We are pleased to inform you that your specialty shop application for <strong>{shopName}</strong> has been <strong>approved</strong>!</p>
            <p><strong>What you can do now:</strong></p>
            <ul>
                <li>Log in to your account with your new ""Specialty Shop"" role</li>
                <li>Access the specialty shop management dashboard</li>
                <li>Update your shop information and opening hours</li>
                <li>Your shop is now available for tour timeline integration</li>
            </ul>
            <p>Welcome to the Tay Ninh Tour specialty shop network!</p>
            <br/>
            <p>Best regards,</p>
            <p>The Tay Ninh Tour Team</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, true);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        /// <summary>
        /// Send email khi SpecialtyShop application bị reject
        /// </summary>
        public async Task SendSpecialtyShopRejectionNotificationAsync(string toEmail, string userName, string shopName, string reason)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(userName, toEmail));
            message.Subject = "Specialty Shop Application - Additional Information Required";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <h2>Hello {userName},</h2>
            <p>Thank you for your interest in joining the Tay Ninh Tour specialty shop network.</p>
            <p>After reviewing your application for <strong>{shopName}</strong>, we need some additional information before we can proceed:</p>
            <div style=""background-color: #f8f9fa; padding: 15px; border-left: 4px solid #dc3545; margin: 20px 0;"">
                <strong>Reason:</strong> {reason}
            </div>
            <p><strong>Next steps:</strong></p>
            <ul>
                <li>Please address the concerns mentioned above</li>
                <li>You can submit a new application once you have the required information</li>
                <li>Contact our support team if you need clarification</li>
            </ul>
            <p>We appreciate your understanding and look forward to working with you.</p>
            <br/>
            <p>Best regards,</p>
            <p>The Tay Ninh Tour Team</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, true);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        /// <summary>
        /// Send email confirmation khi user nộp đơn TourGuide (Enhanced version)
        /// </summary>
        public async Task SendTourGuideApplicationSubmittedAsync(string toEmail, string fullName, DateTime submittedAt)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(fullName, toEmail));
            message.Subject = "Tay Ninh Tour: Tour Guide Application Received";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <h2>Hello {fullName},</h2>
            <p>Thank you for your interest in becoming a Tour Guide with Tay Ninh Tour!</p>
            <p>We have successfully received your application submitted on <strong>{submittedAt:dd/MM/yyyy HH:mm}</strong>.</p>
            <p><strong>What happens next:</strong></p>
            <ul>
                <li>Our team will review your application and CV</li>
                <li>We will contact you within 3-5 business days</li>
                <li>You can check your application status by logging into your account</li>
            </ul>
            <p>If you have any questions, please don't hesitate to contact our support team.</p>
            <br/>
            <p>Best regards,</p>
            <p>The Tay Ninh Tour Team</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, true);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        /// <summary>
        /// Send email khi TourGuide application được approve (Enhanced version)
        /// </summary>
        public async Task SendTourGuideApplicationApprovedAsync(string toEmail, string fullName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(fullName, toEmail));
            message.Subject = "Congratulations! Your Tour Guide Application Has Been Approved";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <h2>Congratulations {fullName}!</h2>
            <p>We are pleased to inform you that your Tour Guide application has been <strong>approved</strong>!</p>
            <p><strong>What you can do now:</strong></p>
            <ul>
                <li>Log in to your account with your new ""Tour Guide"" role</li>
                <li>Access the tour guide management dashboard</li>
                <li>Start accepting tour assignments</li>
                <li>Update your profile and availability</li>
            </ul>
            <p>Welcome to the Tay Ninh Tour guide team!</p>
            <br/>
            <p>Best regards,</p>
            <p>The Tay Ninh Tour Team</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, true);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        /// <summary>
        /// Send email khi TourGuide application bị reject (Enhanced version)
        /// </summary>
        public async Task SendTourGuideApplicationRejectedAsync(string toEmail, string fullName, string reason)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(fullName, toEmail));
            message.Subject = "Tour Guide Application - Additional Information Required";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <h2>Hello {fullName},</h2>
            <p>Thank you for your interest in becoming a Tour Guide with Tay Ninh Tour.</p>
            <p>After careful review, we need additional information regarding your application:</p>
            <blockquote style=""margin: 10px 0; padding: 10px; border-left: 4px solid #ccc; background-color: #f9f9f9;"">
                {reason}
            </blockquote>
            <p><strong>Next steps:</strong></p>
            <ul>
                <li>Please address the concerns mentioned above</li>
                <li>You can submit a new application once you have the required information</li>
                <li>Contact our support team if you need clarification</li>
            </ul>
            <p>We encourage you to apply again and appreciate your interest in joining our team.</p>
            <br/>
            <p>Best regards,</p>
            <p>The Tay Ninh Tour Team</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, true);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

    }
}
