using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;

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

        public const string CookieName = ".AspNet.ApplicationCookie";

        public static string UserName
        {
            get { return ConfigurationManager.AppSettings["username"]; }
        }

        public static string Password
        {
            get { return ConfigurationManager.AppSettings["password"]; }
        }

        public static readonly Lazy<X509Certificate2> Cert = new Lazy<X509Certificate2>(() => new X509Certificate2(ConfigurationManager.AppSettings["x509cert"]));

        public const string HubName = "JudgeHub";

        public const string HubJudge = "Judge";

        public const string HubLockOne = "LockOne";

        public const string HubUpdate = "Update";

        public const string HubUpdateInLock = "UpdateInLock";
    }
}
