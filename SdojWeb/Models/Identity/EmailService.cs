using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace SdojWeb.Models
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            const string mailfrom = "sdcb@live.cn";
            const string password = "***REMOVED***";

            var mail = new MailMessage(mailfrom, message.Destination)
            {
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true,
            };

            var client = new SmtpClient("smtp.live.com", 587)
            {
                Credentials = new NetworkCredential(mailfrom, password),
                EnableSsl = true,
            };

            await client.SendMailAsync(mail);
        }
    }
}