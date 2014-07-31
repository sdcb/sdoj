using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using SdojJudger.Models;

namespace SdojJudger
{
    public class Runner
    {
        public async Task Run()
        {
            // enable ssl custom cert.
            ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidationCallback;

            var authCookie = await AuthenticateUser(AppSettings.UserName, AppSettings.Password);
            
            if (authCookie == null)
            {
                Console.WriteLine("error: login failed for user {0}", AppSettings.UserName);
            }
            else
            {
                var connection = new HubConnection(AppSettings.ServerUrl) { CookieContainer = new CookieContainer() };
                connection.CookieContainer.Add(authCookie);
                Server = connection.CreateHubProxy(AppSettings.HubName);
                Server.On<ClientSolutionPushModel>(AppSettings.HubJudge, OnClientJudge);
                await connection.Start();

                var client = new HubClient(Server);
                var all = await client.GetAll();
                foreach (var clientSolutionPushModel in all)
                {
                    await OnClientJudgeAsync(clientSolutionPushModel);
                }

                Console.WriteLine("welcome " + AppSettings.UserName);
            }
        }

        // Details

        private bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return AppSettings.Cert.Value.Equals(certificate);
        }

        private async Task OnClientJudgeAsync(ClientSolutionPushModel model)
        {
            Console.WriteLine(JsonConvert.SerializeObject(model));
            var ps = new JudgeProcess(model);
            await ps.ExecuteAsync();
        }

        private void OnClientJudge(ClientSolutionPushModel model)
        {
            Console.WriteLine(JsonConvert.SerializeObject(model));
            var ps = new JudgeProcess(model);
            var task = ps.ExecuteAsync();
        }

        private async Task<Cookie> AuthenticateUser(string user, string password)
        {
            var request = WebRequest.CreateHttp(AppSettings.LoginUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();

            var authCredentials = string.Format("username={0}&password={1}",
                WebUtility.UrlEncode(user),
                WebUtility.UrlEncode(password));
            var bytes = Encoding.UTF8.GetBytes(authCredentials);
            request.ContentLength = bytes.Length;

            using (var requestStream = await request.GetRequestStreamAsync())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using (var response = await request.GetResponseAsync() as HttpWebResponse)
            {
// ReSharper disable once PossibleNullReferenceException
                return response.Cookies[AppSettings.CookieName];
            }
        }

        // Fields & Properties.
        public IHubProxy Server { get; private set; }
    }
}
