using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SdojWeb.Models.DbModels;

namespace SdojWeb.Models.ContestModels
{
    public class ContestListModel
    {
        public int Id { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "题目数量")]
        public int Count { get; set; }

        [Display(Name = "时限")]
        public TimeSpan Duration { get; set; }

        [Display(Name = "状态")]
        public ContestStatus Status =>
            CompleteTime != null ? ContestStatus.Completed :
            StartTime == null ? ContestStatus.NotStarted :
            DateTime.Now > StartTime + Duration ? ContestStatus.Completed :
            ContestStatus.Started;

        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

        [Display(Name = "开始时间")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "结束时间")]
        public DateTime? CompleteTime { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ContestListModel other &&
                   Id == other.Id &&
                   Name == other.Name &&
                   Count == other.Count &&
                   CreateTime == other.CreateTime &&
                   Duration == other.Duration &&
                   EqualityComparer<DateTime?>.Default.Equals(StartTime, other.StartTime) &&
                   EqualityComparer<DateTime?>.Default.Equals(CompleteTime, other.CompleteTime);
        }

        public override int GetHashCode()
        {
            var hashCode = 157618039;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Count.GetHashCode();
            hashCode = hashCode * -1521134295 + CreateTime.GetHashCode();
            hashCode = hashCode * -1521134295 + Duration.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<DateTime?>.Default.GetHashCode(StartTime);
            hashCode = hashCode * -1521134295 + EqualityComparer<DateTime?>.Default.GetHashCode(CompleteTime);
            return hashCode;
        }
    }
}