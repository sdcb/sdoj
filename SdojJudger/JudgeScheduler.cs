using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using log4net.Util;
using SdojJudger.Models;

namespace SdojJudger
{
    public class JudgeScheduler : IDisposable
    {
        public JudgeScheduler()
        {
            _queue = new BlockingCollection<SolutionPushModel>();
            _log = LogManager.GetLogger(typeof(JudgeScheduler));
        }

        public void AddOne(SolutionPushModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException();
            }

            if (_queue.Any(x => x.Id == model.Id))
            {
                return;
            }
            _log.InfoExt(() => string.Format("接收推送{0}，语言{1}, 最大内存{2}MB", model.Id, model.Language, model.FullMemoryLimitMb));
            _queue.Add(model);

            if (_task == null)
            {
                _task = Task.Run(() => Loop());
            }
        }

        private async void Loop()
        {
            SolutionPushModel spush;
            while (_queue.TryTake(out spush, 500))
            {
                var ps = new JudgeProcess(spush);
                await ps.ExecuteAsync();
            }
            _task = null;
        }

        private readonly BlockingCollection<SolutionPushModel> _queue;

        private Task _task;

        private readonly ILog _log;

        public void Dispose()
        {
            _queue.Dispose();
        }
    }
}
