using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using SdojJudger.Database;
using SdojJudger.Models;

namespace SdojJudger
{
    public class JudgeProcess
    {
        public JudgeProcess(SolutionPushModel judgeModel)
        {
            _judgeModel = judgeModel;
            _log = LogManager.GetLogger(typeof (JudgeProcess));
        }

        public async Task ExecuteAsync()
        {
            await UpdateQuestionData();
            
        }

        private async Task UpdateQuestionData()
        {
            // 获取并锁定解答的详情。
            _client = App.Runner.GetClient();

            var solution = await _client.Lock(_judgeModel.Id);
            if (solution == null)
            {
                _log.Info("Lock solution failed.");
                return;
            }

            // 与本地数据库对比时间戳。
            var serverItems = solution.QuestionDatas
                .OrderBy(x => x.Id)
                .Select(x => new { x.Id, x.UpdateTime.Ticks })
                .ToArray();
            var ids = solution.QuestionDatas.Select(x => x.Id);

            var db = JudgerDbContext.Create();
            var dbItems = await db.Datas
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, Ticks = x.UpdateTicks })
                .OrderBy(x => x.Id)
                .ToArrayAsync();
            var except = serverItems.Except(dbItems).ToArray();

            var datas = new List<QuestionData>(except.Length);

            // 将旧数据或者没有的数据更新。
            if (except.Length > 0)
            {
                _log.InfoFormat("Found dirty datas, try get {0} data from server.", except.Length);
                var hubDatas = await _client.GetDatas(except.Select(x => x.Id).ToArray());
                datas.AddRange(hubDatas.Select(hubData => new QuestionData
                {
                    Id = hubData.Id,
                    Input = hubData.Input,
                    Output = hubData.Output,
                    MemoryLimitMb = hubData.MemoryLimitMb,
                    TimeLimit = hubData.TimeLimit,
                    UpdateTicks = serverItems.First(x => x.Id == hubData.Id).Ticks
                }));
                db.Datas.AddRange(datas);
                await db.SaveChangesAsync();

                _log.InfoFormat("Updated {0} datas from server.", hubDatas.Length);
                if (datas.Count != datas.Capacity)
                {
                    _log.Warn("Server returned less data than excepted.");
                }
            }
        }

        private readonly SolutionPushModel _judgeModel;

        private readonly ILog _log;

        private HubClient _client;
    }
}
