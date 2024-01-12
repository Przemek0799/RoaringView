using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace SecurityExample.Services
{
    public class EmailSender: IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessasge)
        {
            var FromEmail = "przemekwalkowski1@gmail.com";
            var FromPassword = "toba mttq myyb btio\r\n";

            //Password is from gmail, 2nd verification and theree mobile password
            var message = new MailMessage();
            message.From = new MailAddress(FromEmail);
            message.Subject = subject;
            message.Body = htmlMessasge;
            message.IsBodyHtml = true;
            message.To.Add(email);
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(FromEmail, FromPassword),
                EnableSsl = true
        };
            smtpClient.Send(message);
            
        }
    }
}
