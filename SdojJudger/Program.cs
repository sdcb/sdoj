using System;
using System.Net;
using Microsoft.AspNet.SignalR.Client;

namespace SdojJudger
{
    class Program
    {
        static void Main(string[] args)
        {
            var connection = new HubConnection(AppSettings.ServerUrl);
            Cookie returnedCookie;

            var authResult = AuthenticateUser(AppSettings.UserName, AppSettings.Password, out returnedCookie);

            if (authResult)
            {
                connection.CookieContainer = new CookieContainer();
                connection.CookieContainer.Add(returnedCookie);
                Console.WriteLine("Welcome " + AppSettings.UserName);
            }
            else
            {
                Console.WriteLine("Login failed");
            }

            var hub = connection.CreateHubProxy(AppSettings.HubName);
            hub.On("DoWork", str => Console.WriteLine(str));

            connection.Start().Wait();

            while (true)
            {
                var line = Console.ReadLine();
                hub.Invoke("Send", line);
            }
        }

        private static bool AuthenticateUser(string user, string password, out Cookie authCookie)
        {
            var request = WebRequest.Create(AppSettings.LoginUrl) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();

            var authCredentials = "username=" + user + "&password=" + password;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(authCredentials);
            request.ContentLength = bytes.Length;
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using (var response = request.GetResponse() as HttpWebResponse)
            {
                authCookie = response.Cookies[AppSettings.CookieName];
            }

            if (authCookie != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}