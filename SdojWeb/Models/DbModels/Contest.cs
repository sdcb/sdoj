﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SdojWeb.Models.DbModels
{
    public class Contest
    {
        public int Id { get; set; }

        [MaxLength(30)]
        public string Name { get; set; }

        public bool Public { get; set; }

        public ContestStatus Status
        {
            get
            {
                if (StartTime == null) return ContestStatus.NotStarted;
                if (CompleteTime == null) return ContestStatus.Started;
                return ContestStatus.Completed;
            }
        }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? CompleteTime { get; set; }

        public ICollection<ContestQuestion> Questions { get; set; }

        public ICollection<ContestUser> Users { get; set; }
    }

    public class ContestQuestion
    {
        public int Id { get; set; }

        public int ContestId { get; set; }

        public int QuestionId { get; set; }

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

    public enum ContestStatus
    {
        NotStarted, 
        Started, 
        Completed, 
    }
}