using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using AutoMapper;
using SdojWeb.Infrastructure.Mapping;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SdojWeb.Models
{
    public class Question : IMapFrom<QuestionNotMappedEditModel>
    {
        [HiddenInput]
        public int Id { get; set; }

        [HiddenInput]
        public int CreateUserId { get; set; }

        [HiddenInput]
        public ApplicationUser CreateUser { get; set; }


        [Display(Name = "标题"), Required, MaxLength(30), Index(IsUnique = true)]
        public string Name { get; set; }

        [Display(Name = "描述"), Required, MaxLength(4000), DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "示例输入"), MaxLength(4000), DataType(DataType.MultilineText)]
        public string SampleInput { get; set; }

        [Display(Name = "示例输出"), Required, MaxLength(4000), DataType(DataType.MultilineText)]
        public string SampleOutput { get; set; }

        [Display(Name = "内存限制(MB)"), DefaultValue(64)]
        public float MemoryLimitMb { get; set; }

        [Display(Name = "时间限制(ms)"), DefaultValue(1000)]
        public int TimeLimit { get; set; }

        [Display(Name = "创建时间"), HiddenInput]
        public DateTime CreateTime { get; set; }

        [Display(Name = "更新时间"), HiddenInput, Index]
        public DateTime UpdateTime { get; set; }
    }

    public class QuestionCreateModel : IHaveCustomMapping
    {
        [Display(Name = "标题"), Required, MaxLength(30)]
        public string Name { get; set; }

        [Display(Name = "描述"), Required, MaxLength(4000), DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "示例输入"), MaxLength(4000), DataType(DataType.MultilineText)]
        public string SampleInput { get; set; }

        [Display(Name = "示例输出"), Required, MaxLength(4000), DataType(DataType.MultilineText)]
        public string SampleOutput { get; set; }

        [Display(Name = "内存限制(MB)"), DefaultValue(64)]
        public float MemoryLimitMb { get; set; }

        [Display(Name = "时间限制(ms)"), DefaultValue(1000)]
        public int TimeLimit { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionCreateModel, Question>()
                .ForMember(source => source.CreateTime, dest => dest.MapFrom(x => DateTime.Now))
                .ForMember(source => source.CreateUserId, dest => dest.MapFrom(x => HttpContext.Current.User.Identity.GetIntUserId()))
                .ForMember(source => source.UpdateTime, dest => dest.MapFrom(x => DateTime.Now));
        }
    }

    public class QuestionEditModel : IHaveCustomMapping, IMapFrom<Question>
    {
        [Editable(false)]
        public int Id { get; set; }

        [Display(Name = "标题"), Required, MaxLength(30)]
        public string Name { get; set; }

        [Display(Name = "描述"), Required, MaxLength(4000), DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "示例输入"), MaxLength(4000), DataType(DataType.MultilineText)]
        public string SampleInput { get; set; }

        [Display(Name = "示例输出"), Required, MaxLength(4000), DataType(DataType.MultilineText)]
        public string SampleOutput { get; set; }

        [Display(Name = "内存限制(MB)"), DefaultValue(64)]
        public float MemoryLimitMb { get; set; }

        [Display(Name = "时间限制(ms)"), DefaultValue(1000)]
        public int TimeLimit { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionEditModel, Question>()
                .ForMember(source => source.UpdateTime, dest => dest.MapFrom(x => DateTime.Now));
        }
    }

    public class QuestionNotMappedEditModel : IMapFrom<Question>
    {
        public int CreateUserId { get; set; }

        public DateTime CreateTime { get; set; }
    }

    public class QuestionSummaryViewModel : IMapFrom<Question>
    {
        [Index]
        public int Id { get; set; }

        [Display(Name = "标题")]
        public string Name { get; set; }

        [Display(Name = "内存限制(MB)"), DisplayFormat(DataFormatString = "{0:F2}")]
        public float MemoryLimitMb { get; set; }

        [Display(Name = "时间限制(ms)")]
        public int TimeLimit { get; set; }

        [Display(Name = "更新时间")]
        public DateTime UpdateTime { get; set; }
    }
}