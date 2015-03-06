using System.Threading.Tasks;
using log4net;
using log4net.Util;
using Microsoft.VisualBasic.Devices;
using SdojJudger.Compiler.Infrastructure;
using SdojJudger.Models;

namespace SdojJudger.Business
{
    public class JudgeProcess
    {
        public JudgeProcess(SolutionPushModel spush)
        {
            _spush = spush;
            _log = LogManager.GetLogger(typeof(JudgeProcess));
            _client = App.Starter.GetClient();
        }

        public async Task ExecuteAsync()
        {
            if (!PrecheckEnvironment(_spush))
            {
                return;
            }

            var judger = JudgeDriver.Create(_spush);
            await judger.ExecuteAsync();
        }

        private bool PrecheckEnvironment(SolutionPushModel _spush)
        {
            if (!CompilerProvider.IsLanguageAvailable(_spush))
            {
                _log.InfoExt(() => string.Format("Skipped compiling {0}, Because {1} compiler is not availabel.",
                        _spush.Id, _spush.Language));
                return false;
            }
            var info = new ComputerInfo();
            if (info.AvailablePhysicalMemory < _spush.FullMemoryLimitMb * 1024 * 1024)
            {
                _log.InfoExt(
                    () =>
                        string.Format("Skipped judging {0}, because system memory running low(Req {1}/ Need {2}).",
                            _spush.Id, info.AvailablePhysicalMemory, _spush.FullMemoryLimitMb * 1024 * 1024)
                    );
                return false;
            }
            return true;
        }

        private readonly SolutionPushModel _spush;

        private readonly ILog _log;

        private readonly HubClient _client;
    }
}
