using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace RealEstate_ServicesSystem.Utilities
{
    public class EmailSender: IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("medo66reda6677@gmail.com", "ikzp kqtj wgsj qvzo"),
                UseDefaultCredentials = false,
                EnableSsl = true
            };

            try
            {
                return client.SendMailAsync(
                    new MailMessage("medo66reda6677@gmail.com",
                    email,
                    subject,
                    htmlMessage)
                    { IsBodyHtml = true });
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error sending email: {ex.Message}");
                return Task.CompletedTask;
            }
        }
    }
}
