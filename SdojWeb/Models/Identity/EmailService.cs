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
            var from = AppSettings.EmailFrom;
            var password = AppSettings.EmailPassword;
            var host = AppSettings.SmtpHost;
            var port = AppSettings.SmtpPort;

            var mail = new MailMessage(from, message.Destination)
            {
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true,
            };

            using (var client = new SmtpClient(host, port))
            {
                client.Credentials = new NetworkCredential(from, password);
                client.EnableSsl = true;

                await client.SendMailAsync(mail);
            }
        }
    }
}