using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using SdojWeb.Models;

namespace SdojWeb.Infrastructure.Identity
{
    public class JudgeClientAuthorize : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var request = httpContext.Request;

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
                    var loginModel = JsonConvert.DeserializeObject<LoginViewModel>(plaintext);
                    var user = UserManager.Find(loginModel.Email, loginModel.Password);

                    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>{new Claim(ClaimTypes.NameIdentifier, user.Id.ToStringInvariant())}));
                }
            }
            return false;
        }

        public ApplicationUserManager UserManager { get; set; }
    }
}