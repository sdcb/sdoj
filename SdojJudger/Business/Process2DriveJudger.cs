using log4net;
using log4net.Util;
using SdojJudger.Models;
using System;
using System.Threading.Tasks;

namespace SdojJudger.Business
{
    public class Process2DriveJudger : JudgeDriver
    {
        public Process2DriveJudger(SolutionPushModel spush) : base()
        {
            _log = LogManager.GetLogger(GetType());
            _client = App.Starter.GetClient();
            _spush = spush;
        }

        public override async Task ExecuteAsync()
        {
            _lockM = await _client.LockProcess2(_spush.Id);
            if (_lockM == null)
            {
                _log.InfoExt($"Failed to lock {_spush.Id}, move next.");
                return;
            }
            await UpdateQuestionProcess2Code();
        }

        private Task UpdateQuestionProcess2Code()
        {
            throw new NotImplementedException();
        }

        private readonly ILog _log;
        private readonly HubClient _client;
        private readonly SolutionPushModel _spush;

        private Process2LockModel _lockM;
        private QuestionProcess2FullModel _fullM;
    }
}