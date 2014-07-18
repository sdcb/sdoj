using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace SdojJudger
{
    class Program
    {
        static void Main(string[] args)
        {
            var connection = new HubConnection(AppSettings.ServerUrl);

            var authCookie = AuthenticateUser(AppSettings.UserName, AppSettings.Password).Result;

            if (authCookie != null)
            {
                connection.CookieContainer = new CookieContainer();
                connection.CookieContainer.Add(authCookie);
                Console.WriteLine("Welcome " + AppSettings.UserName);

                var hub = connection.CreateHubProxy(AppSettings.HubName);
                hub.On("DoWork", str => Console.WriteLine(str));

                connection.Start().Wait();

                while (true)
                {
                    var line = Console.ReadLine();
                    hub.Invoke("Send", line);
                }
            }
            else
            {
                Console.WriteLine("Login failed");
            }
        }

        private static async Task<Cookie> AuthenticateUser(string user, string password)
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
                return response.Cookies[AppSettings.CookieName];
            }
        }
    }
}