using SdojJudger.Models;

namespace SdojJudger
{
    public class JudgeProcess
    {
        public JudgeProcess(ClientJudgeModel judgeModel)
        {
            _judgeModel = judgeModel;
        }

        public void Execute()
        {
            
        }

        private ClientJudgeModel _judgeModel;
    }
}
