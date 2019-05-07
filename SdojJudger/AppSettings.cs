using System.Configuration;

namespace SdojJudger
{
    public static class AppSettings
    {
        public static string ServerUrl
            => ConfigurationManager.AppSettings["serverUrl"];

        public static string LoginUrl
            => ServerUrl + "Account/LoginAsJudger";

        public const string CookieName = ".AspNet.ApplicationCookie";

        public static string UserName
            => ConfigurationManager.AppSettings["username"];

        public static string Password
            => ConfigurationManager.AppSettings["password"];

        public static string VcCommandline
            => ConfigurationManager.AppSettings["VcCommandline"];

        public static string GccPath
            => ConfigurationManager.AppSettings["GccPath"];

        public static string Python3Path
            => ConfigurationManager.AppSettings["Python3Path"];

        public static string NodeExePath 
            => ConfigurationManager.AppSettings[nameof(NodeExePath)];

        public static string JdkBinPath
            => ConfigurationManager.AppSettings[nameof(JdkBinPath)];

        public static float JavaGivenMemoryMb 
            => int.Parse(ConfigurationManager.AppSettings[nameof(JavaGivenMemoryMb)]);

        public static string HubGetProcess2Code = "GetProcess2Code";

        public static string HubLockProcess2 = "LockProcess2";

        public const string HubName = "JudgeHub";

        public const string HubJudge = "Judge";

        public const string HubLock = "Lock";

        public const string HubUpdate = "Update";

        public const string HubUpdateInLock = "UpdateInLock";

        public const string HubGetAll = "GetAll";

        public const string HubGetDatas = "GetDatas";
    }
}
