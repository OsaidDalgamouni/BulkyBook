using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace BulkyBookweb.Models.utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailTosend = new MimeMessage();
            emailTosend.From.Add(MailboxAddress.Parse("dalqmoni33@gmail.com"));
            emailTosend.To.Add(MailboxAddress.Parse(email));
            emailTosend.Subject = subject;
            emailTosend.Body= new TextPart(MimeKit.Text.TextFormat.Html) { Text=htmlMessage };
            //send email
            using (var emailClient=new SmtpClient())
            {
                emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                emailClient.Authenticate("bullkybook@gmail.com", "tcgxzvzmtepyyixi");
                emailClient.Send(emailTosend);
                emailClient.Disconnect(true);
            }
            return Task.CompletedTask;
        }
    }
}
