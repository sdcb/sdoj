using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SdojWeb.Models.DbModels
{
    public class Question2ndProcessDriveCode
    {
        [Key]
        public int Id { get; set; }

        [Index(IsClustered = true)]
        public int QuestionId { get; set; }

        [Required]
        public string Code { get; set; }

        public Languages Language { get; set; }

        public int TimeLimitMs { get; set; }

        public float MemoryLimitMb { get; set; }

        public DateTime UpdateTime { get; set; }

        public Question Question { get; set; }
    }
}