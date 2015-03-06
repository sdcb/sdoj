using log4net;
using SdojJudger.Models;
using System.Threading.Tasks;

namespace SdojJudger.Business
{
    public abstract class JudgeDriver
    {
        public abstract Task ExecuteAsync();

        public static JudgeDriver Create(SolutionPushModel spush)
        {
            if (spush.QuestionType == QuestionTypes.DataDrive)
            {
                return new DataDriveJudger(spush);
            }
            else if (spush.QuestionType == QuestionTypes.Process2Drive)
            {
                return new Process2DriveJudger(spush);
            }
            else
            {
                return new NothingJudgeDriver(spush);
            }
        }
    }

    public class NothingJudgeDriver : JudgeDriver
    {
        public NothingJudgeDriver(SolutionPushModel spush)
        {
            _spush = spush;
        }

        public override Task ExecuteAsync()
        {
            log.Error($"No judger for {_spush.QuestionType}, {_spush.Id} failed.");
            return Task.FromResult(0);
        }

        private ILog log = LogManager.GetLogger(typeof(NothingJudgeDriver));
        private readonly SolutionPushModel _spush;
    }
}
