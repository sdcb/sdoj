using SdojWeb.Infrastructure.Mapping;
using System;
using System.ComponentModel.DataAnnotations;

namespace SdojWeb.Models.DbModels
{
    public class Process2JudgeCode : IMapFrom<QuestionProcess2CodeEditModel>
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public string Code { get; set; }

        public Languages Language { get; set; }

        public short RunTimes { get; set; }

        public int TimeLimitMs { get; set; }

        public float MemoryLimitMb { get; set; }

        public DateTime UpdateTime { get; set; }

        public Question Question { get; set; }
    }
}