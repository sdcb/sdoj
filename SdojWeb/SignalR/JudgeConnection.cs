using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using SdojWeb.Models;

namespace SdojWeb.SignalR
{
    public class JudgeConnection : PersistentConnection
    {
        public ApplicationUserManager UserManager
        {
            get { return DependencyResolver.Current.GetService<ApplicationUserManager>(); }
        }

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            var user = request.Environment["UserId"] as int?;
            if (user != null)
            {
                Groups.Add(connectionId, user.ToString());
            }
            return Connection.Send(connectionId, "Hello");
        }

        protected override Task OnDisconnected(IRequest request, string connectionId)
        {
            var user = request.Environment["UserId"] as int?;
            if (user != null)
            {
                Groups.Remove(connectionId, user.ToString());
            }
            return base.OnDisconnected(request, connectionId);
        }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return Connection.Broadcast(data);
        }

        protected override bool AuthorizeRequest(IRequest request)
        {
            var cipherString = request.Headers["Security-Token"];
            var allBytes = Convert.FromBase64String(cipherString);
            var iv = new byte[16];
            var cipher = new byte[allBytes.Length - iv.Length];
            Buffer.BlockCopy(allBytes, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(allBytes, iv.Length, cipher, 0, cipher.Length);
            using (var aes = new AesCryptoServiceProvider())
            {
                using (var decryptor = aes.CreateDecryptor(AppSettings.ClientKey, iv))
                {
                    var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
                    var userId = BitConverter.ToInt32(plainBytes, 0);
                    request.Environment["UserId"] = userId;
                    return true;
                }
            }
        }
    }
}