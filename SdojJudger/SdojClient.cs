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
    public class SdojClient
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
                Console.WriteLine("welcome " + AppSettings.UserName);

                Console.ReadKey();
            }
        }

        public async Task<ClientSolutionFullModel> Lock(int solutionId)
        {
            return await Server.Invoke<ClientSolutionFullModel>(
                AppSettings.HubLock, solutionId);
        }

        public async Task<bool> Update(int solutionId,
            SolutionStatus statusId, int? runTimeMs, float? usingMemoryMb)
        {
            return await Server.Invoke<bool>(AppSettings.HubUpdate,
                solutionId, statusId, runTimeMs, usingMemoryMb);
        }

        public async Task<bool> UpdateInLock(int solutionId, SolutionStatus statusId)
        {
            return await Server.Invoke<bool>(AppSettings.HubUpdateInLock,
                solutionId, statusId);
        }

        public async Task<ClientSolutionPushModel[]> GetAll()
        {
            return await Server.Invoke<ClientSolutionPushModel[]>(
                AppSettings.HubGetAll);
        }

        // Details

        private bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return AppSettings.Cert.Value.Equals(certificate);
        }

        private void OnClientJudge(ClientSolutionPushModel model)
        {
            Console.WriteLine(JsonConvert.SerializeObject(model));
            var ps = new JudgeProcess(model);
            ps.Execute();
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
        private IHubProxy Server { get; set; }
    }
}
