using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Util;
using Microsoft.AspNet.SignalR.Client;
using SdojJudger.Models;

namespace SdojJudger
{
    public class Starter
    {
        public Starter()
        {
            _log = LogManager.GetLogger(typeof(Starter));
        }

        public async Task Run()
        {
            var authCookie = await AuthenticateUser(AppSettings.UserName, AppSettings.Password);

            if (authCookie[AppSettings.CookieName] == null)
            {
                _log.FatalFormat("error: login failed for user {0}", AppSettings.UserName);
                throw new AuthenticationException();
            }
            else
            {
                _hub = new HubConnection(AppSettings.ServerUrl) {CookieContainer = new CookieContainer()};
                _hub.CookieContainer.Add(authCookie);

                _hub.Closed += async () =>
                {
                    _log.WarnExt(() => "Connection Closed");
                    await Restart();
                };

                _proxy = _hub.CreateHubProxy(AppSettings.HubName);
                _proxy.On<SolutionPushModel>(AppSettings.HubJudge, OnClientJudge);
                
                _judger = new JudgeScheduler();
                await _hub.Start();

                _log.InfoFormat("{0} online.", AppSettings.UserName);

                var client = GetClient();
                var all = await client.GetAll();
                foreach (var clientSolutionPushModel in all)
                {
                    OnClientJudge(clientSolutionPushModel);
                }
            }
        }

        public async Task Restart()
        {
            try
            {
                if (Interlocked.CompareExchange(ref _restarting, 1, 0) == 0)
                {
                    _log.InfoExt(() => "Restarting...");
                    if (_hub != null) _hub.Dispose();
                    if (_judger != null) _judger.Dispose();
                    await Run();
                }
            }
            finally
            {
                var result = Interlocked.CompareExchange(ref _restarting, 0, 1);
                // Assert result == 0
            }
            
        }

        public HubClient GetClient()
        {
            return new HubClient(_proxy);
        }

        // Details
        
        private void OnClientJudge(SolutionPushModel model)
        {
            _judger.AddOne(model);
        }

        private async Task<CookieCollection> AuthenticateUser(string user, string password)
        {
            var handler = new HttpClientHandler {UseCookies = true};
            var httpClient = new HttpClient(handler);
            var parameters = new Dictionary<string, string>
            {
                {"username", user}, 
                {"password", password}
            };
            var response = await httpClient.PostAsync(AppSettings.LoginUrl, new FormUrlEncodedContent(parameters));
            if (((int) response.StatusCode/100) == 5) // 500 501 502 503....
            {
                response.EnsureSuccessStatusCode(); // throw a exception.
            }

            return handler.CookieContainer.GetCookies(new Uri(AppSettings.LoginUrl));
        }

        // Fields & Properties.

        private JudgeScheduler _judger;

        private IHubProxy _proxy;

        private HubConnection _hub;

        private readonly ILog _log;

        private int _restarting = 0;
    }
}
