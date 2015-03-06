using SdojJudger.Models;
using System;
using System.Threading.Tasks;

namespace SdojJudger.Business
{
    public class Process2DriveJudger : JudgeDriver
    {
        public Process2DriveJudger(SolutionPushModel spush) : base()
        {
            _spush = spush;
        }

        public override Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }

        private readonly SolutionPushModel _spush;
    }
}