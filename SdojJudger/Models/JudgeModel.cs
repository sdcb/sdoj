using System.Collections.Generic;

namespace SdojJudger.Models
{
    public class JudgeModel
    {
        public int SolutionId { get; set; }

        public int QuestionId { get; set; }

        public string Source { get; set; }

        public Languages Language { get; set; }

        public int TimeLimit { get; set; }

        public long MemoryLimit { get; set; }

        public int QuestionCreateUserId { get; set; }

        public List<QuestionDataHashModel> QuestionDatas { get; set; }
    }
}
