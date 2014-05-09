using System.Collections.Generic;

namespace SdojWeb.Models
{
    public class JudgeModel
    {
        public int SolutionId { get; set; }

        public string Source { get; set; }

        public Languages Language { get; set; }

        public int TimeLimit { get; set; }

        public long MemoryLimitBytes { get; set; }

        public List<QuestionData> QuestionDatas { get; set; }
    }
}