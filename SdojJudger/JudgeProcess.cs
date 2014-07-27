using SdojJudger.Models;

namespace SdojJudger
{
    public class JudgeProcess
    {
        public JudgeProcess(ClientSolutionPushModel judgeModel)
        {
            _judgeModel = judgeModel;
        }

        public void Execute()
        {
            
        }

        private ClientSolutionPushModel _judgeModel;
    }
}
