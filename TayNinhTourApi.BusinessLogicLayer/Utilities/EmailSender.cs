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

        /// <summary>
        /// Common method to send email with proper SMTP configuration and error handling
        /// </summary>
        private async Task SendEmailAsync(MimeMessage message)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    // Set LocalDomain to avoid Unicode hostname issues
                    client.LocalDomain = "localhost.localdomain";

                    // Connect using StartTLS for Gmail port 587
                    await client.ConnectAsync(
                        _emailSettings.SmtpServer,
                        _emailSettings.SmtpPort,
                        MailKit.Security.SecureSocketOptions.StartTls
                    );

                    // Authenticate using Username from settings
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
            catch (Exception ex)
            {
                // Log the error với thông tin chi tiết
                var toEmail = message.To.FirstOrDefault()?.ToString() ?? "Unknown";
                var subject = message.Subject ?? "No subject";
                
                Console.WriteLine($"=== EMAIL SENDING FAILED ===");
                Console.WriteLine($"To: {toEmail}");
                Console.WriteLine($"Subject: {subject}");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Exception Type: {ex.GetType().Name}");
                
                // Log SMTP settings (nhưng không log password)
                Console.WriteLine($"SMTP Server: {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");
                Console.WriteLine($"Username: {_emailSettings.Username}");
                Console.WriteLine($"Sender: {_emailSettings.SenderName} <{_emailSettings.SenderEmail}>");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Console.WriteLine("=== END EMAIL ERROR ===");

                // THROW LẠI EXCEPTION để caller có thể handle
                throw new InvalidOperationException($"Failed to send email to {toEmail}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Public method to send email with string parameters
        /// </summary>
        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            message.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(message);
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

            await SendEmailAsync(message);
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

            await SendEmailAsync(message);
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

            await SendEmailAsync(message);
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

            await SendEmailAsync(message);
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

            await SendEmailAsync(message);
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

            await SendEmailAsync(message);
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

            await SendEmailAsync(message);
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

            await SendEmailAsync(message);
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

            await SendEmailAsync(message);
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

            await SendEmailAsync(message);
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

            await SendEmailAsync(message);
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

            await SendEmailAsync(message);
        }

        /// <summary>
        /// Send invitation email to TourGuide for a TourDetails
        /// </summary>
        public async Task SendTourGuideInvitationAsync(string toEmail, string guideName, string tourTitle, string tourCompanyName, DateTime expiresAt, string invitationId)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(guideName, toEmail));
            message.Subject = "Tour Guide Invitation - New Tour Assignment Available";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <h2>Hello {guideName},</h2>
            <p>You have received a new tour assignment invitation!</p>
            <div style=""background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;"">
                <h3 style=""color: #2c3e50; margin-top: 0;"">Tour Details:</h3>
                <p><strong>Tour:</strong> {tourTitle}</p>
                <p><strong>Tour Company:</strong> {tourCompanyName}</p>
                <p><strong>Invitation Expires:</strong> {expiresAt:dd/MM/yyyy HH:mm}</p>
            </div>
            <p><strong>What you can do:</strong></p>
            <ul>
                <li>Log in to your account to view full tour details</li>
                <li>Accept or decline the invitation</li>
                <li>View tour timeline and requirements</li>
            </ul>
            <p style=""color: #e74c3c;""><strong>Important:</strong> This invitation will expire on {expiresAt:dd/MM/yyyy} at {expiresAt:HH:mm}. Please respond before the deadline.</p>
            <p>We look forward to your response!</p>
            <br/>
            <p>Best regards,</p>
            <p>The Tay Ninh Tour Team</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(message);
        }

        /// <summary>
        /// Send confirmation email when TourGuide is assigned to TourDetails
        /// </summary>
        public async Task SendGuideAssignmentConfirmationAsync(string toEmail, string guideName, string tourTitle, string tourCompanyName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(guideName, toEmail));
            message.Subject = "Tour Assignment Confirmed - You've Been Selected!";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <h2>Congratulations {guideName}!</h2>
            <p>You have been successfully assigned as the tour guide for the following tour:</p>
            <div style=""background-color: #d4edda; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #28a745;"">
                <h3 style=""color: #155724; margin-top: 0;"">Tour Assignment Details:</h3>
                <p><strong>Tour:</strong> {tourTitle}</p>
                <p><strong>Tour Company:</strong> {tourCompanyName}</p>
                <p><strong>Status:</strong> Awaiting Admin Approval</p>
            </div>
            <p><strong>Next steps:</strong></p>
            <ul>
                <li>Your assignment is now pending admin approval</li>
                <li>You will receive another notification once approved</li>
                <li>Log in to your account to view tour details and timeline</li>
                <li>Prepare for the tour according to the requirements</li>
            </ul>
            <p>Thank you for accepting this tour assignment!</p>
            <br/>
            <p>Best regards,</p>
            <p>The Tay Ninh Tour Team</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(message);
        }

        /// <summary>
        /// Send notification to admin when TourDetails needs approval
        /// </summary>
        public async Task SendAdminApprovalRequestAsync(string toEmail, string adminName, string tourTitle, string tourCompanyName, string guideName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(adminName, toEmail));
            message.Subject = "Admin Action Required - Tour Assignment Approval";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <h2>Hello {adminName},</h2>
            <p>A new tour assignment requires your approval:</p>
            <div style=""background-color: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;"">
                <h3 style=""color: #856404; margin-top: 0;"">Tour Assignment Details:</h3>
                <p><strong>Tour:</strong> {tourTitle}</p>
                <p><strong>Tour Company:</strong> {tourCompanyName}</p>
                <p><strong>Assigned Guide:</strong> {guideName}</p>
                <p><strong>Status:</strong> Awaiting Admin Approval</p>
            </div>
            <p><strong>Action required:</strong></p>
            <ul>
                <li>Log in to the admin panel</li>
                <li>Review the tour details and guide assignment</li>
                <li>Approve or reject the assignment</li>
                <li>Provide feedback if rejecting</li>
            </ul>
            <p>Please review this assignment at your earliest convenience.</p>
            <br/>
            <p>Best regards,</p>
            <p>The Tay Ninh Tour System</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(message);
        }

        /// <summary>
        /// Send notification when TourDetails is cancelled due to no guide assignment
        /// </summary>
        public async Task SendTourDetailsCancellationAsync(string toEmail, string companyName, string tourTitle, string reason)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(companyName, toEmail));
            message.Subject = "Tour Assignment Cancelled - No Guide Available";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <h2>Hello {companyName},</h2>
            <p>We regret to inform you that your tour assignment has been cancelled:</p>
            <div style=""background-color: #f8d7da; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #dc3545;"">
                <h3 style=""color: #721c24; margin-top: 0;"">Cancelled Tour Details:</h3>
                <p><strong>Tour:</strong> {tourTitle}</p>
                <p><strong>Reason:</strong> {reason}</p>
                <p><strong>Status:</strong> Cancelled</p>
            </div>
            <p><strong>What happened:</strong></p>
            <ul>
                <li>No tour guide accepted the invitation within the required timeframe</li>
                <li>The system automatically cancelled the assignment after 5 days</li>
                <li>You can create a new tour assignment with different requirements</li>
            </ul>
            <p><strong>Suggestions for future assignments:</strong></p>
            <ul>
                <li>Consider adjusting the skills requirements</li>
                <li>Offer competitive compensation</li>
                <li>Provide more detailed tour information</li>
                <li>Contact our support team for assistance</li>
            </ul>
            <p>We apologize for any inconvenience and look forward to helping you with future tour assignments.</p>
            <br/>
            <p>Best regards,</p>
            <p>The Tay Ninh Tour Team</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(message);
        }

        /// <summary>
        /// Send invitation email to SpecialtyShop for a TourDetails
        /// </summary>
        public async Task SendSpecialtyShopTourInvitationAsync(string toEmail, string shopName, string ownerName, string tourTitle, string tourCompanyName, DateTime tourDate, DateTime expiresAt, string invitationId)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(shopName, toEmail));
            message.Subject = "Tour Partnership Invitation - Join Our Tour Experience";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <h2>Hello {ownerName},</h2>
            <p>We are excited to invite <strong>{shopName}</strong> to participate in an upcoming tour experience!</p>

            <div style=""background-color: #e8f5e8; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #28a745;"">
                <h3 style=""color: #155724; margin-top: 0;"">Tour Invitation Details:</h3>
                <p><strong>Tour:</strong> {tourTitle}</p>
                <p><strong>Tour Company:</strong> {tourCompanyName}</p>
                <p><strong>Tour Date:</strong> {tourDate:dd/MM/yyyy}</p>
                <p><strong>Invitation ID:</strong> {invitationId}</p>
                <p><strong>Response Deadline:</strong> {expiresAt:dd/MM/yyyy HH:mm}</p>
            </div>

            <p><strong>What this means for your shop:</strong></p>
            <ul>
                <li>Your shop will be featured in the tour timeline</li>
                <li>Potential customers will visit your shop during the tour</li>
                <li>Opportunity to showcase your products and services</li>
                <li>Increased visibility and sales potential</li>
                <li>Partnership with established tour companies</li>
            </ul>

            <p><strong>Next Steps:</strong></p>
            <ul>
                <li>Review the tour details and timeline</li>
                <li>Prepare your shop for potential tour visitors</li>
                <li>Respond to this invitation before the deadline</li>
                <li>Contact the tour company if you have questions</li>
            </ul>

            <p><strong>How to respond:</strong></p>
            <ul>
                <li>Log in to your Specialty Shop dashboard</li>
                <li>Navigate to ""Tour Invitations"" section</li>
                <li>Accept or decline this invitation</li>
                <li>Add any special notes or requirements</li>
            </ul>

            <p>This is a great opportunity to grow your business and connect with tourists visiting Tay Ninh!</p>
            <br/>
            <p>Best regards,</p>
            <p>The Tay Ninh Tour Team</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(message);
        }

        /// <summary>
        /// Send tour booking confirmation email with QR code
        /// </summary>
        public async Task SendTourBookingConfirmationAsync(
            string toEmail,
            string customerName,
            string bookingCode,
            string tourTitle,
            DateTime tourDate,
            int numberOfGuests,
            decimal totalPrice,
            string contactPhone,
            byte[] qrCodeImage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(customerName, toEmail));
            message.Subject = "Tour Booking Confirmed - Your QR Code Ticket";

            var bodyBuilder = new BodyBuilder();

            // Add QR code as embedded image
            var qrCodeAttachment = bodyBuilder.Attachments.Add("qr-code.png", qrCodeImage, new ContentType("image", "png"));
            qrCodeAttachment.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
            qrCodeAttachment.ContentId = "qr-code";

            bodyBuilder.HtmlBody = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
                <div style=""text-align: center; margin-bottom: 30px;"">
                    <h1 style=""color: #2c3e50; margin-bottom: 10px;"">🎉 Booking Confirmed!</h1>
                    <p style=""color: #7f8c8d; font-size: 16px;"">Thank you for choosing Tay Ninh Tour</p>
                </div>

                <div style=""background-color: #d4edda; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #28a745;"">
                    <h2 style=""color: #155724; margin-top: 0; text-align: center;"">Your Tour Booking Details</h2>

                    <table style=""width: 100%; border-collapse: collapse; margin-top: 15px;"">
                        <tr>
                            <td style=""padding: 8px 0; font-weight: bold; color: #155724;"">Booking Code:</td>
                            <td style=""padding: 8px 0; color: #155724;"">{bookingCode}</td>
                        </tr>
                        <tr>
                            <td style=""padding: 8px 0; font-weight: bold; color: #155724;"">Tour:</td>
                            <td style=""padding: 8px 0; color: #155724;"">{tourTitle}</td>
                        </tr>
                        <tr>
                            <td style=""padding: 8px 0; font-weight: bold; color: #155724;"">Date:</td>
                            <td style=""padding: 8px 0; color: #155724;"">{tourDate:dd/MM/yyyy}</td>
                        </tr>
                        <tr>
                            <td style=""padding: 8px 0; font-weight: bold; color: #155724;"">Number of Guests:</td>
                            <td style=""padding: 8px 0; color: #155724;"">{numberOfGuests}</td>
                        </tr>
                        <tr>
                            <td style=""padding: 8px 0; font-weight: bold; color: #155724;"">Total Price:</td>
                            <td style=""padding: 8px 0; color: #155724; font-weight: bold;"">{totalPrice:N0} VND</td>
                        </tr>
                        <tr>
                            <td style=""padding: 8px 0; font-weight: bold; color: #155724;"">Contact Phone:</td>
                            <td style=""padding: 8px 0; color: #155724;"">{contactPhone}</td>
                        </tr>
                    </table>
                </div>

                <div style=""text-align: center; margin: 30px 0;"">
                    <h3 style=""color: #2c3e50; margin-bottom: 15px;"">📱 Your QR Code Ticket</h3>
                    <div style=""background-color: #f8f9fa; padding: 20px; border-radius: 8px; display: inline-block;"">
                        <img src=""cid:qr-code"" alt=""QR Code Ticket"" style=""width: 200px; height: 200px; border: 2px solid #dee2e6; border-radius: 8px;"" />
                    </div>
                    <p style=""color: #6c757d; font-size: 14px; margin-top: 10px;"">
                        Present this QR code to your tour guide on the day of your tour
                    </p>
                </div>

                <div style=""background-color: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;"">
                    <h4 style=""color: #856404; margin-top: 0;"">📋 Important Information:</h4>
                    <ul style=""color: #856404; margin: 10px 0; padding-left: 20px;"">
                        <li>Please arrive 15 minutes before the tour start time</li>
                        <li>Bring a valid ID for verification</li>
                        <li>Keep this QR code accessible on your phone or print it out</li>
                        <li>Contact us if you need to make any changes to your booking</li>
                    </ul>
                </div>

                <div style=""text-align: center; margin: 30px 0; padding: 20px; background-color: #f8f9fa; border-radius: 8px;"">
                    <h4 style=""color: #2c3e50; margin-bottom: 15px;"">Need Help?</h4>
                    <p style=""color: #6c757d; margin: 5px 0;"">📞 Phone: +84 123 456 789</p>
                    <p style=""color: #6c757d; margin: 5px 0;"">📧 Email: support@tayninhtravel.com</p>
                    <p style=""color: #6c757d; margin: 5px 0;"">🌐 Website: www.tayninhtravel.com</p>
                </div>

                <div style=""text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6;"">
                    <p style=""color: #6c757d; font-size: 14px;"">We look forward to providing you with an amazing tour experience!</p>
                    <br/>
                    <p style=""color: #2c3e50; font-weight: bold;"">Best regards,</p>
                    <p style=""color: #2c3e50; font-weight: bold;"">The Tay Ninh Tour Team</p>
                </div>
            </div>";

            message.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(message);
        }

    }
}
