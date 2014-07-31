using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SdojJudger.Models;

namespace SdojJudger
{
    public class JudgeProcess
    {
        public JudgeProcess(ClientSolutionPushModel judgeModel)
        {
            _judgeModel = judgeModel;
        }

        public async Task ExecuteAsync()
        {
            var client = App.Runner.GetClient();
            var result = await client.Lock(_judgeModel.Id);
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        private readonly ClientSolutionPushModel _judgeModel;
    }
}
