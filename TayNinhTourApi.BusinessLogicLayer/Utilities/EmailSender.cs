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

    }
}
