using System;
using System.ComponentModel.DataAnnotations;
using SdojWeb.Infrastructure.Mapping;

namespace SdojWeb.Models
{
    public class QuestionData
    {
        public int Id { get; set; }

        public virtual Question Question { get; set; }

        [Required]
        public int QuestionId { get; set; }

        public string Input { get; set; }

        [Required]
        public string Output { get; set; }

        [Required]
        public DateTime UpdateTime { get; set; }
    }

    public class QuestionDataSummaryModel : IMapFrom<QuestionData>
    {
        public int Id { get; set; }

        [Display(Name = "题目")]
        public string QuestionName { get; set; }

        public int QuestionId { get; set; }

        [Display(Name = "输入字符数")]
        public int InputLength { get; set; }

        [Display(Name = "输出字符数")]
        public int OutputLength { get; set; }

        [Display(Name = "编辑日期")]
        public DateTime UpdateTime { get; set; }
    }
}