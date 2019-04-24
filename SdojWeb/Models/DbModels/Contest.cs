using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SdojWeb.Models.DbModels
{
    public class Contest
    {
        public int Id { get; set; }

        [MaxLength(30), Required]
        public string Name { get; set; }

        public bool Public { get; set; }

        public TimeSpan Duration { get; set; }

        public ContestStatus Status =>
            CompleteTime != null ? ContestStatus.Completed :
            StartTime == null ? ContestStatus.NotStarted :
            DateTime.Now > StartTime + Duration ? ContestStatus.Completed :
            ContestStatus.Started;

        public bool CompletedButNoCompleteTime =>
            Status == ContestStatus.Completed && CompleteTime == null;

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? CompleteTime { get; set; }

        public ICollection<ContestQuestion> Questions { get; set; }

        public ICollection<ContestUser> Users { get; set; }

        public int CreateUserId { get; set; }

        public User CreateUser { get; set; }

        public ICollection<ContestSolution> Solutions { get; set; }
    }

    public class ContestQuestion
    {
        public int Id { get; set; }

        public int ContestId { get; set; }

        public int QuestionId { get; set; }

        public int Rank { get; set; }

        public Contest Contest { get; set; }

        public Question Question { get; set; }
    }

    public class ContestUser
    {
        public int Id { get; set; }

        public int ContestId { get; set; }

        public Contest Contest { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
    }

    public class ContestSolution
    {
        public int Id { get; set; }

        public int ContestId { get; set; }

        public Contest Contest { get; set; }

        public int SolutionId { get; set; }

        public Solution Solution { get; set; }
    }

    public enum ContestStatus
    {
        [Display(Name = "未开始")]
        NotStarted, 
        [Display(Name = "进行中")]
        Started, 
        [Display(Name = "已结束")]
        Completed, 
    }
}