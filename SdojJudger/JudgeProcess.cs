using System;
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

        public async void Execute()
        {
            var result = await Program.Client.Lock(_judgeModel.Id);
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        private readonly ClientSolutionPushModel _judgeModel;
    }
}
