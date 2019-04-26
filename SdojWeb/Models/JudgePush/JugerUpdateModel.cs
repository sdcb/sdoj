using System;
using System.Threading.Tasks;
using SdojWeb.Models.DbModels;

namespace SdojWeb.Models.JudgePush
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

        public void UpdateSolution(Solution solution)
        {
            solution.State = StateId;
            solution.RunTime = RunTimeMs;
            solution.UsingMemoryMb = UsingMemoryMb;
            solution.CompilerOutput = CompilerOutput?.Substring(0, Math.Min(Solution.CompilerOutputLimit, CompilerOutput.Length));
            if (WrongAnswer != null)
            {
                var wrongAnswer = solution.WrongAnswer ?? new SolutionWrongAnswer();
                {
                    wrongAnswer.QuestionDataId = WrongAnswerInputId;
                    wrongAnswer.Output = WrongAnswer?.Substring(0, Math.Min(SolutionWrongAnswer.WrongAnswerLimit, WrongAnswer.Length));
                }
                solution.WrongAnswer = wrongAnswer;
            }
            solution.Lock = null;
        }
    }
}