using SdojWeb.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SdojWeb.Models
{
    public class Question
    {
        [HiddenInput]
        public int Id { get; set; }

        [Display(Name = "标题"), Required, MaxLength(30)]
        public string Name { get; set; }

        [Display(Name = "描述"), Required, MaxLength(4000), DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "示例输入"), MaxLength(4000), DataType(DataType.MultilineText)]
        public string SampleInput { get; set; }

        [Display(Name = "示例输出"), Required, MaxLength(4000), DataType(DataType.MultilineText)]
        public string SampleOutput { get; set; }

        [Display(Name = "内存限制"), DefaultValue(64)]
        public double MemoryLimitMB { get; set; }

        [Display(Name = "时间限制"), DefaultValue(1000)]
        public int TimeLimit { get; set; }
    }

    public class QuestionSummaryViewModel : IMapFrom<Question>
    {
        public int Id { get; set; }

        [Display(Name = "标题")]
        public string Name { get; set; }

        [Display(Name = "内存限制(MB)"), DisplayFormat(DataFormatString = "{0:F2}")]
        public double MemoryLimitMB { get; set; }

        [Display(Name = "时间限制(ms)")]
        public int TimeLimit { get; set; }
    }
}