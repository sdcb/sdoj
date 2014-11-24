using System;
using System.ComponentModel.DataAnnotations;

namespace SdojWeb.Models.DbModels
{
    public class Solution
    {
        public int Id { get; set; }

        public User CreateUser { get; set; }

        public int CreateUserId { get; set; }

        public Question Question { get; set; }

        public int QuestionId { get; set; }

        public Languages Language { get; set; }

        [Required]
        [MaxLength(32 * 1024)]
        public string Source { get; set; }

        [MaxLength(CompilerOutputLength)]
        public string CompilerOutput { get; set; }

        public SolutionState State { get; set; }

        public float UsingMemoryMb { get; set; }

        public int RunTime { get; set; }

        public DateTime SubmitTime { get; set; }

        public SolutionLock Lock { get; set; }

        public const int CompilerOutputLength = 500;
    }
}