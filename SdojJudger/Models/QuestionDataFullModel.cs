using System;

namespace SdojJudger.Models
{
    public class QuestionDataFullModel
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }

        public DateTime UpdateTime { get; set; }

        public string Input { get; set; }

        public string Output { get; set; }

        public float MemoryLimitMb { get; set; }

        public int TimeLimit { get; set; }
    }
}
