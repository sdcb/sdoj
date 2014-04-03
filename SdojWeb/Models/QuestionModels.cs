using SdojWeb.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SdojWeb.Models
{
    public class Question : IHaveCustomMapping
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(80, ErrorMessage="{0} 不能超过 {1} 个字符。")]
        [Display(Name = "标题")]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000, ErrorMessage = "{0} 不能超过 {1} 个字符。")]
        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "示例输入")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000, ErrorMessage = "{0} 不能超过 {1} 个字符。")]
        public string SampleInput { get; set; }

        [Required]
        [Display(Name = "示例输出")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000, ErrorMessage = "{0} 不能超过 {1} 个字符。")]
        public string SampleOutput { get; set; }

        [Display(Name = "内存限制")]
        public long MemoryLimit { get; set; }

        [Display(Name = "时间限制")]
        public long TimeLimit { get; set; }

        public void CreateMappings(AutoMapper.IConfiguration configuration)
        {
            configuration.CreateMap<Question, QuestionSummaryViewModel>()
                .ForMember(source => source.MemoryLimitMB, dest => dest.MapFrom(i => i.MemoryLimit * 1.0 / 1024 / 1024));

            configuration.CreateMap<Question, QuestionDetailViewModel>()
                .ForMember(source => source.MemoryLimitMB, desc => desc.MapFrom(i => i.MemoryLimit * 1.0 / 1024 / 1024));
        }
    }

    public class QuestionSummaryViewModel
    {
        public int Id { get; set; }

        [Display(Name = "标题")]
        public string Title { get; set; }

        [Display(Name = "内存限制(MB)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double MemoryLimitMB { get; set; }

        [Display(Name = "时间限制(ms)")]
        public long TimeLimit { get; set; }
    }

    public class QuestionDetailViewModel : IHaveCustomMapping
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(80, ErrorMessage = "{0} 不能超过 {1} 个字符。")]
        [Display(Name = "标题")]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000, ErrorMessage = "{0} 不能超过 {1} 个字符。")]
        [Display(Name = "描述")]
        public string Description { get; set; }

        [Display(Name = "示例输入")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000, ErrorMessage = "{0} 不能超过 {1} 个字符。")]
        public string SampleInput { get; set; }

        [Required]
        [Display(Name = "示例输出")]
        [DataType(DataType.MultilineText)]
        [MaxLength(4000, ErrorMessage = "{0} 不能超过 {1} 个字符。")]
        public string SampleOutput { get; set; }

        [Display(Name = "内存限制(MB)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double MemoryLimitMB { get; set; }

        [Display(Name = "时间限制(ms)")]
        public long TimeLimit { get; set; }

        public void CreateMappings(AutoMapper.IConfiguration configuration)
        {
            configuration.CreateMap<QuestionDetailViewModel, Question>()
                .ForMember(dest => dest.MemoryLimit, source => source.MapFrom(i => i.MemoryLimitMB * 1024 * 1024));
        }
    }
}