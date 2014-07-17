﻿using System.Configuration;

namespace SdojJudger
{
    public static class AppSettings
    {
        public static string ServerUrl
        {
            get { return ConfigurationManager.AppSettings["serverUrl"]; }
        }

        public static string LoginUrl
        {
            get { return ServerUrl + "Account/LoginAsJudger"; }
        }

        public const string HubName = "JudgeHub";

        public const string CookieName = ".AspNet.ApplicationCookie";

        public static string UserName
        {
            get { return ConfigurationManager.AppSettings["username"]; }
        }

        public static string Password
        {
            get { return ConfigurationManager.AppSettings["password"]; }
        }
    }
}
