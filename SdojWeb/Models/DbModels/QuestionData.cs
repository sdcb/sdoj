using System;
using System.ComponentModel.DataAnnotations;

namespace SdojWeb.Models.DbModels
{
    public class QuestionData
    {
        public int Id { get; set; }

        public Question Question { get; set; }

        public int QuestionId { get; set; }

        public string Input { get; set; }

        [Required]
        public string Output { get; set; }

        public float MemoryLimitMb { get; set; }

        public int TimeLimit { get; set; }

        public DateTime UpdateTime { get; set; }

        public bool IsSample { get; set; }
    }
}