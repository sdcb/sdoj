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
            // no-reply@sdcb.in
            var from = AppSettings.EmailFrom;

            // 123456
            var password = AppSettings.EmailPassword;

            // stmp.ym.163.com
            var host = AppSettings.SmtpHost;

            // 25
            var port = AppSettings.SmtpPort;
            
            // Destination为收件箱，如397482054@qq.com
            var mail = new MailMessage(from, message.Destination)
            {
                Subject = message.Subject, // 标题
                Body = message.Body, // 内容
                IsBodyHtml = true, // 内容是否为HTML格式
            };

            using (var client = new SmtpClient(host, port))
            {
                // from是你的邮箱地址；password是密码
                client.Credentials = new NetworkCredential(from, password);
                client.EnableSsl = true;

                // 发送
                await client.SendMailAsync(mail);
            }
        }
    }
}