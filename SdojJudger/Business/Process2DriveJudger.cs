using log4net;
using log4net.Util;
using SdojJudger.Database;
using SdojJudger.Models;
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

        private async Task UpdateQuestionProcess2Code()
        {
            var questionId = _spush.QuestionId;

            using (var db = new JudgerDbContext())
            {
                var contains = await db.ContainsProcess2Code(questionId);
                
                bool needUpdate = true;
                if (contains)
                {
                    var item = (DbHashModel)_lockM.JudgeCode;
                    var dbItem = await db.FindProcess2HashById(questionId);

                    if (dbItem.UpdateTicks == item.UpdateTicks)
                    {
                        needUpdate = false;
                    }
                }

                if (needUpdate)
                {
                    // 数据过期，需要执行更新操作。
                    if (contains)
                        _log.Info("P2Code expired, updating from server.");
                    else
                        _log.Info("P2Code not exist, fetching from server.");

                    var hubItem = await _client.GetProcess2Code(questionId);
                    var toDbItem = (QuestionP2Code)hubItem;
                    await db.DeleteAndCreateProcess2Code(toDbItem);
                }
            }
        }

        private readonly ILog _log;
        private readonly HubClient _client;
        private readonly SolutionPushModel _spush;

        private Process2LockModel _lockM;
        private QuestionProcess2FullModel _fullM;
    }
}