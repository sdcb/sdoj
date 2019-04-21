using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdojJudger.Models
{
    public class ClientJudgeModel
    {
        public int SolutionId { get; set; }
        public SolutionState StateId { get; set; }
        public int RunTimeMs { get; set; }
        public float UsingMemoryMb { get; set; }
        public string CompilerOutput { get; set; }
        public int WrongAnswerInputId { get; set; }
        public string WrongAnswer { get; set; }
        public string WrongAnswerInput { get; set; }

        public static ClientJudgeModel CreateCompileError(int solutionId, string compilerOutput)
        {
            const int CompilerOutputLimit = 500;
            if (compilerOutput?.Length > CompilerOutputLimit) compilerOutput = compilerOutput.Substring(0, CompilerOutputLimit);
            return new ClientJudgeModel
            {
                SolutionId = solutionId, 
                StateId = SolutionState.CompileError, 
                CompilerOutput = compilerOutput
            };
        }

        public static ClientJudgeModel Create(int solutionId, SolutionState solutionState, int runTimeMs, float peakMemoryMb)
        {
            return new ClientJudgeModel
            {
                SolutionId = solutionId,
                StateId = solutionState,
                RunTimeMs = runTimeMs, 
                UsingMemoryMb = peakMemoryMb, 
            };
        }

        public static ClientJudgeModel CreateWrongAnswer(int solutionId, int runTimeMs, float peakMemoryMb, int wrongAnswerInputId, string wrongAnswer)
        {
            const int WrongAnswerLimit = 1000;
            if (wrongAnswer?.Length > WrongAnswerLimit) wrongAnswer = wrongAnswer.Substring(0, WrongAnswerLimit);
            return new ClientJudgeModel
            {
                SolutionId = solutionId,
                StateId = SolutionState.WrongAnswer,
                RunTimeMs = runTimeMs,
                UsingMemoryMb = peakMemoryMb,
                WrongAnswerInputId = wrongAnswerInputId, 
                WrongAnswer = wrongAnswer, 
            };
        }

        public static ClientJudgeModel CreateProcess2WrongAnswer(int solutionId, int runTimeMs, float peakMemoryMb, string wrongAnswerInput, string wrongAnswer)
        {
            const int WrongAnswerLimit = 1000;
            if (wrongAnswerInput?.Length > WrongAnswerLimit) wrongAnswerInput = wrongAnswerInput.Substring(0, WrongAnswerLimit);
            if (wrongAnswer?.Length > WrongAnswerLimit) wrongAnswer = wrongAnswer.Substring(0, WrongAnswerLimit);
            return new ClientJudgeModel
            {
                SolutionId = solutionId,
                StateId = SolutionState.WrongAnswer,
                RunTimeMs = runTimeMs,
                UsingMemoryMb = peakMemoryMb,
                WrongAnswerInput = wrongAnswerInput, 
                WrongAnswer = wrongAnswer,
            };
        }
    }
}
