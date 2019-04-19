using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SdojWeb.Models.DbModels;

namespace SdojWeb.Manager
{
    public class ContestListModel
    {
        public int Id { get; }

        [Display(Name = "名称")]
        public string Name { get; }

        [Display(Name = "题目数量")]
        public int Count { get; }

        [Display(Name = "时限")]
        public TimeSpan Duration { get; set; }

        [Display(Name = "状态")]
        public ContestStatus Status =>
            CompleteTime != null ? ContestStatus.Completed :
            StartTime == null ? ContestStatus.NotStarted :
            StartTime + Duration > DateTime.Now ? ContestStatus.Completed :
            ContestStatus.Started;

        [Display(Name = "创建时间")]
        public System.DateTime CreateTime { get; }

        [Display(Name = "开始时间")]
        public System.DateTime? StartTime { get; }

        [Display(Name = "结束时间")]
        public System.DateTime? CompleteTime { get; }

        public ContestListModel(int id, string name, int count, DateTime createTime, DateTime? startTime, System.DateTime? completeTime)
        {
            Id = id;
            Name = name;
            Count = count;
            CreateTime = createTime;
            StartTime = startTime;
            CompleteTime = completeTime;
        }

        public override bool Equals(object obj)
        {
            return obj is ContestListModel other &&
                   Id == other.Id &&
                   Name == other.Name &&
                   Count == other.Count &&
                   CreateTime == other.CreateTime &&
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
            hashCode = hashCode * -1521134295 + EqualityComparer<DateTime?>.Default.GetHashCode(StartTime);
            hashCode = hashCode * -1521134295 + EqualityComparer<DateTime?>.Default.GetHashCode(CompleteTime);
            return hashCode;
        }
    }
}