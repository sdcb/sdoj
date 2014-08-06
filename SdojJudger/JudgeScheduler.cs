using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using SdojJudger.Models;

namespace SdojJudger
{
    public class JudgeScheduler : IDisposable
    {
        public JudgeScheduler()
        {
            _queue = new BlockingCollection<SolutionPushModel>();
        }

        public void AddOne(SolutionPushModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException();
            }

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

        public void Dispose()
        {
            _queue.Dispose();
        }
    }
}
