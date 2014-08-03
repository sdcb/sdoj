using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SdojJudger.Models;

namespace SdojJudger
{
    public class JudgeProcess
    {
        public JudgeProcess(SolutionPushModel judgeModel)
        {
            _judgeModel = judgeModel;
        }

        public async Task ExecuteAsync()
        {
            var client = App.Runner.GetClient();
            var result = await client.Lock(_judgeModel.Id);
        }

        private readonly SolutionPushModel _judgeModel;
    }
}
