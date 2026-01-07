using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Infrastructure.Data;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Identity.Client;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Runtime;
using Microsoft.VisualBasic;
using Microsoft.Extensions.Options;
using Application.Interfaces;

namespace Application.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly SMSDbContext _context;
        private readonly IEmailService _emailService;
        private readonly SendGridSettings _settings;

        public AuthServices(SMSDbContext context, IEmailService emailService, IOptions<SendGridSettings> settings)
        {
            _context = context;
            _emailService = emailService;
            _settings = settings.Value;
        }
        public LoginResponse Login(string email, string password)
        {
            var response = new LoginResponse();
            var user = _context.Bandas.FirstOrDefault(u => u.Ban_username == email);

            if (user == null)
            {
                response.Success = false;
                response.Message = "Invalid login credentials.";
                return response;
            }

            if (!user.BanActive)
            {
                response.Success = false;
                response.IsActive = false;
                response.Message = "Your account is inactive. Please contact the administration.";
                return response;
            }

            if (verifyPassword(password, user.BanPassword))
            {
                markAsLoggedIn(user);
                response.Success = true;
                response.IsActive = true;
                user.BanFailedAttempts = 0;
                _context.Update(user);
                _context.SaveChanges();

                response.User = user; // ✅ pass back Banda object
                return response;
            }

            updateBanFailedAttempts(user);
            response.Success = false;
            response.Message = "Invalid login credentials.";
            return response;
        }


        //== VERIFY PASSWORD
        private bool verifyPassword(String password,String storedPassword)
        {
            if(password==null) return false;
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, storedPassword);
            return isPasswordCorrect;
        }
        // == UPDATE LOGIN
        private void markAsLoggedIn(Banda user)
        {
            user.BanCurrentlyLogin = true;
        }
        // UPDATE NUMBER OF FAILED ATTEMPTS
        private void updateBanFailedAttempts(Banda user)
        {
            user.BanFailedAttempts++;
            if (user.BanFailedAttempts >= 2)
            {
                user.BanActive = false;
            }
            _context.Update(user);
            _context.SaveChanges();
        }
        //============ SIGNUP
        bool  IAuthServices.Register(string email, string password)
        {
            bool usernameExists = _context.Bandas.Any(b => b.Ban_username == email);
            if (usernameExists)
            {
                return false;
            }
            else
            {
                //  Hashing password 
                password = BCrypt.Net.BCrypt.HashPassword(password);
                var user = new Banda
                {
                    Ban_username = email,
                    BanPassword = password,
                    BanActive = true,
                    BanRole = "student",
                    BanCurrentlyLogin = false,
                    BanFailedAttempts = 0,
                    BanCreatedAt= DateTime.UtcNow,
                };
                _context.Bandas.Add(user);
                _context.SaveChangesAsync();
                return true;
            }
        }

        public async Task SendActivationEmail(string toEmail, string activationLink)
        {
            var client = new SendGridClient(_settings.ApiKey);
            var from = new EmailAddress(_settings.SenderEmail, _settings.SenderName);
            var to = new EmailAddress(toEmail);
            var subject = "You’re Ready – Start your Admission Journey with Deshani Scholers School";

            var plainTextContent = $"Dear Sir/Madam,\r\n\r\n" +
                                   $"We are very excited to inform you that your account on the Deshani Scholers Online Admission Portal has been successfully created and you are now ready to take the next step.\r\n\r\n" +
                                   $"You can now log in to begin or continue your application process. From your dashboard, you’ll be able to:\r\n- Complete your admission form\r\n- " +
                                   $"Upload supporting documents\r\n- " +
                                   $"Check the progress of your application\r\n- " +
                                   $"Get notified about any updates or next steps\r\n- " +
                                   $"Seek assistance when needed\r\n\r\n" +
                                   $"Login here: {activationLink}  \r\n" +
                                   $"(Use the email and password you registered with.)\r\n\r\n" +
                                   $"Need help or have questions? Our admissions team is happy to assist. Reach out anytime at [Contact Email] or [Phone Number].\r\n\r\n" +
                                   $"We appreciate that you’ve taken this first step and look forward to reviewing your application!\r\n\r\n" +
                                   $"Warm regards,  \r\n" +
                                   $"Admissions Coordinator  \r\n" +
                                   $"[Deshani Scholers School]  \r\n" +
                                   $"[Website URL]  \r\n" +
                                   $"[Email Address] | [Phone Number]\r\n";

            // HTML version
            var htmlContent = $@"
              <p>Dear Sir/Madam,</p>

            <p>We are very excited to inform you that your account on the <strong>School Management System Online Admission Portal</strong> has been successfully created, and you are now ready to take the next step.</p>
            
            <p>You can now log in to begin or continue your application process. From your dashboard, you’ll be able to:</p>
            <ul>
              <li>Complete your admission form</li>
              <li>Upload supporting documents</li>
              <li>Check the progress of your application</li>
              <li>Get notified about any updates or next steps</li>
              <li>Seek assistance when needed</li>
            </ul>
            
            <p>
              <a href=""{activationLink}"" style=""background-color:#B22222;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;margin-top:10px;"">
                🔗 Log in here
              </a>
            </p>
            <p>(Use the email and password you registered with.)</p>
            
            <p>Need help or have questions? Our admissions team is happy to assist. Reach out anytime at <a href=""mailto:[Contact Email]"">[Contact Email]</a> or call [Phone Number].</p>
            
            <p>We appreciate that you’ve taken this first step and look forward to reviewing your application!</p>
            
            <p>
            Warm regards,<br>
            Admissions Coordinator<br>
            Deshani Scholers School<br>
            <a href=""[Website URL]"">[Website URL]</a><br>
            <a href=""mailto:[Email Address]"">[Email Address]</a> | [Phone Number]
            </p>
            
    ";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            msg.ReplyTo = new EmailAddress(_settings.SenderEmail);
            await client.SendEmailAsync(msg);
        }

        public async Task SendResetPasswordEmail(string toEmail, string resetLink)
        {
            var client = new SendGridClient(_settings.ApiKey);
            var from = new EmailAddress(_settings.SenderEmail, _settings.SenderName);
            var to = new EmailAddress(toEmail);
            var subject = "Reset Your Password";
            var plainTextContent = $"To reset your password, please click the link below:\r\n{resetLink}\r\nIf you did not request a password reset, please ignore this email.";
            var htmlContent = $@"
                <p>To reset your password, please click the link below:</p>
                <p>
                    <a href=""{resetLink}"" style=""background-color:#B22222;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;margin-top:10px;"">
                        Reset Password
                    </a>
                </p>
                <p>If you did not request a password reset, please ignore this email.</p>
            ";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            msg.ReplyTo = new EmailAddress(_settings.SenderEmail);
            await client.SendEmailAsync(msg);
        }
    }

}
