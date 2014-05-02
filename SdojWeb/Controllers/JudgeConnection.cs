using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.ClientServices;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.Owin.Security;
using SdojWeb.Models;

namespace SdojWeb.Controllers
{
    public class JudgeConnection : PersistentConnection
    {
        public ApplicationUserManager UserManager
        {
            get { return DependencyResolver.Current.GetService<ApplicationUserManager>(); }
        }

        private static readonly AutoResetEvent RunOnce = new AutoResetEvent(true);

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            if (RunOnce.WaitOne(0))
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        Connection.Broadcast(DateTime.Now.ToString("O"));
                        Thread.Sleep(500);
                    }
                });
            }
            return Connection.Send(connectionId, "Welcome!");
        }

        protected override bool AuthorizeRequest(IRequest request)
        {
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
                    return user != null && user.EmailConfirmed;
                }
            }
        }

        protected override Task OnDisconnected(IRequest request, string connectionId)
        {
            return base.OnDisconnected(request, connectionId);
        }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return Connection.Broadcast(data);
        }
    }
}