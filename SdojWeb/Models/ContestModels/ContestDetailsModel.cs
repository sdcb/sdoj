﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SdojWeb.Models.DbModels;

namespace SdojWeb.Models.ContestModels
{
    public class ContestDetailsModel
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
            StartTime + Duration > DateTime.Now ? ContestStatus.Completed :
            ContestStatus.Started;

        [Display(Name = "创建时间")]
        public DateTime CreateTime { get; set; }

        [Display(Name = "开始时间")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "结束时间")]
        public DateTime? CompleteTime { get; set; }

        public List<string> Questions { get; set; }
    }
}