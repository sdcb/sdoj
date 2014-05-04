using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Json;
using SdojWeb.Models;

namespace SdojWeb.Controllers
{
    public class JudgeConnection : PersistentConnection
    {
        public ApplicationUserManager UserManager
        {
            get { return DependencyResolver.Current.GetService<ApplicationUserManager>(); }
        }

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            var user = request.Environment["User"] as ApplicationUser;
            if (user != null)
            {
                Groups.Add(connectionId, user.UserName);
            }
            return Connection.Send(connectionId, "Hello");
        }

        protected override Task OnDisconnected(IRequest request, string connectionId)
        {
            var user = request.Environment["User"] as ApplicationUser;
            if (user != null)
            {
                Groups.Remove(connectionId, user.UserName);
            }
            return base.OnDisconnected(request, connectionId);
        }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return Connection.Broadcast(data);
        }

        protected override bool AuthorizeRequest(IRequest request)
        {
            var httpMethod = (string)request.Environment["owin.RequestMethod"];
            if (httpMethod == "POST")
            {
                // 只允许客户端连接，但禁止客户端发消息；
                // 因为SignalR消息使用了同步机制，会转发到所有网站实例上。
                return request.Headers["Private-Key"] == AppSettings.PrivateKey;
            }

            var clientPublicKey = Convert.FromBase64String(request.Headers["Public-Key"]);
            var clientIv = Convert.FromBase64String(request.Headers["IV"]);
            var securityToken = Convert.FromBase64String(request.Headers["Security-Token"]);

            using (var ecd = new ECDiffieHellmanCng(
                CngKey.Import(Convert.FromBase64String(AppSettings.PrivateKey), CngKeyBlobFormat.EccPrivateBlob)))
            {
                var agreement = ecd.DeriveKeyMaterial(
                    ECDiffieHellmanCngPublicKey.FromByteArray(clientPublicKey,
                    CngKeyBlobFormat.EccPublicBlob));
                var aes = new AesCryptoServiceProvider();
                using (var decryptor = aes.CreateDecryptor(agreement, clientIv))
                {
                    var plainbytes = decryptor.TransformFinalBlock(securityToken, 0, securityToken.Length);
                    var plaintext = Encoding.Unicode.GetString(plainbytes);
                    var loginModel = JsonSerializer.Parse<LoginViewModel>(plaintext);
                    var user = UserManager.Find(loginModel.Email, loginModel.Password);

                    if (user != null && user.EmailConfirmed)
                    {
                        request.Environment.Add("User", user);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}