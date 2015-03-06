using SdojJudger.Models;
using System;
using System.Threading.Tasks;

namespace SdojJudger.Business
{
    public abstract class JudgeDriver
    {
        public abstract Task ExecuteAsync();

        public static JudgeDriver Create(SolutionPushModel spush)
        {
            if (spush.QuestionType == QuestionTypes.DataDrive)
            {
                return new DataDriveJudger(spush);
            }
            else if (spush.QuestionType == QuestionTypes.Process2Drive)
            {
                throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }
}
