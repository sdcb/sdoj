using System.Linq;
using System.Web;
using AutoMapper;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Infrastructure.Mapping;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SdojWeb.Models.DbModels;

namespace SdojWeb.Models
{
    public class QuestionDetailModel : IHaveCustomMapping
    {
        [HiddenInput]
        public int Id { get; set; }

        [Display(Name = "标题")]
        public string Name { get; set; }

        [Display(Name = "描述"), DataType("Markdown")]
        public string Description { get; set; }

        [Display(Name = "输入说明"), DataType("Markdown"), DisplayFormat(NullDisplayText = "无")]
        public string InputExplain { get; set; }

        [Display(Name = "输出说明"), DataType("Markdown"), DisplayFormat(NullDisplayText = "无")]
        public string OutputExplain { get; set; }

        [Display(Name = "总内存限制"), DisplayFormat(DataFormatString = "{0} MB")]
        public float MemoryLimitMb { get; set; }

        [Display(Name = "总时间限制"), DisplayFormat(DataFormatString = "{0} ms")]
        public int TimeLimit { get; set; }

        [Display(Name = "输入样例"), DataType(DataType.MultilineText)]
        public string SampleInput { get; set; }

        [Display(Name = "输出样例"), DataType(DataType.MultilineText)]
        public string SampleOutput { get; set; }

        public int CreateUserId { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<Question, QuestionDetailModel>()
                .ForMember(s => s.SampleInput, d => d.MapFrom(x => x.SampleData.Input))
                .ForMember(s => s.SampleOutput, d => d.MapFrom(x => x.SampleData.Output))
                .ForMember(s => s.MemoryLimitMb, d => d.MapFrom(x => x.Datas.Max(v => v.MemoryLimitMb)))
                .ForMember(s => s.TimeLimit, d => d.MapFrom(x => x.Datas.Sum(v => v.TimeLimit)));
        }
    }

    public class QuestionCreateModel : IHaveCustomMapping
    {
        [Display(Name = "标题"), Required, Remote("CheckName", "Question"), MaxLength(30)]
        public string Name { get; set; }

        [Display(Name = "描述"), Required, MaxLength(4000), DataType("Markdown")]
        public string Description { get; set; }

        [Display(Name = "输入说明"), MaxLength(1000), DataType("Markdown")]
        public string InputExplain { get; set; }

        [Display(Name = "输出说明"), MaxLength(1000), DataType("Markdown")]
        public string OutputExplain { get; set; }

        [Display(Name = "示例输入"), MaxLength(4000), DataType(DataType.MultilineText)]
        public string SampleInput { get; set; }

        [Display(Name = "示例输出"), Required, MaxLength(4000), DataType(DataType.MultilineText)]
        public string SampleOutput { get; set; }

        [Display(Name = "内存限制(MB)"), Required]
        public float MemoryLimitMb { get; set; }

        [Display(Name = "时间限制(ms)"), Required]
        public int TimeLimit { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionCreateModel, Question>()
                .ForMember(source => source.CreateTime, dest => dest.MapFrom(x => DateTime.Now))
                .ForMember(source => source.CreateUserId, dest => dest.MapFrom(x => HttpContext.Current.User.Identity.GetIntUserId()))
                .ForMember(source => source.UpdateTime, dest => dest.MapFrom(x => DateTime.Now));
            configuration.CreateMap<QuestionCreateModel, QuestionData>()
                .ForMember(s => s.Input, d => d.MapFrom(x => x.SampleInput))
                .ForMember(s => s.Output, d => d.MapFrom(x => x.SampleOutput))
                .ForMember(s => s.MemoryLimitMb, d => d.MapFrom(x => x.MemoryLimitMb))
                .ForMember(s => s.TimeLimit, d => d.MapFrom(x => x.TimeLimit))
                .ForMember(s => s.UpdateTime, d => d.MapFrom(x => DateTime.Now));
        }
    }

    public class QuestionEditModel : IHaveCustomMapping
    {
        [Editable(false)]
        public int Id { get; set; }

        [Display(Name = "标题"), Required, MaxLength(30)]
        public string Name { get; set; }

        [Display(Name = "描述"), Required, MaxLength(4000), DataType("Markdown")]
        public string Description { get; set; }

        [Display(Name = "内存限制(MB)"), Editable(false)]
        public float MemoryLimitMb { get; set; }

        [Display(Name = "时间限制(ms)"), Editable(false)]
        public int TimeLimit { get; set; }

        [HiddenInput]
        public int? QuestionDataId { get; set; }

        [Display(Name = "输入说明"), MaxLength(1000), DataType("Markdown")]
        public string InputExplain { get; set; }

        [Display(Name = "输出说明"), MaxLength(1000), DataType("Markdown")]
        public string OutputExplain { get; set; }

        [Display(Name = "输入样例"), DataType(DataType.MultilineText)]
        public string SampleInput { get; set; }

        [Display(Name = "输出样例"), Required, DataType(DataType.MultilineText)]
        public string SampleOutput { get; set; }

        [HiddenInput]
        public int CreateUserId { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionEditModel, Question>()
                .ForMember(source => source.UpdateTime, dest => dest.MapFrom(x => DateTime.Now));
            configuration.CreateMap<Question, QuestionEditModel>()
                .ForMember(source => source.SampleInput, dest => dest.MapFrom(x => x.SampleData.Input))
                .ForMember(source => source.SampleOutput, dest => dest.MapFrom(x => x.SampleData.Output))
                .ForMember(source => source.QuestionDataId, dest => dest.MapFrom(x => x.SampleDataId))
                .ForMember(source => source.CreateUserId, dest => dest.MapFrom(x => x.CreateUserId))
                .ForMember(s => s.MemoryLimitMb, d => d.MapFrom(x => x.Datas.Max(v => v.MemoryLimitMb)))
                .ForMember(s => s.TimeLimit, d => d.MapFrom(x => x.Datas.Sum(v => v.TimeLimit))); 
        }
    }

    public class QuestionNotMappedEditModel : IMapFrom<Question>
    {
        public int CreateUserId { get; set; }

        public DateTime CreateTime { get; set; }

        public int? SampleDataId { get; set; }

        public QuestionData SampleData { get; set; }
    }

    public class QuestionSummaryBasicViewModel : IHaveCustomMapping
    {
        [HiddenInput]
        public int Id { get; set; }

        [Display(Name = "标题")]
        public string Name { get; set; }

        [Display(Name = "作者")]
        public string Creator { get; set; }

        [Display(Name = "更新时间")]
        public DateTime UpdateTime { get; set; }

        [Display(Name = "测试数据"), DisplayFormat(DataFormatString = "{0}个")]
        public int DataCount { get; set; }

        [Display(Name = "通过/解答")]
        public int SolutionCount { get; set; }

        public int AcceptedCount { get; set; }

        [Display(Name = "内存限制(MB)")]
        public float MemoryLimitMb { get; set; }

        [Display(Name = "时间限制(ms)")]
        public int TimeLimit { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<Question, QuestionSummaryBasicViewModel>()
                .ForMember(s => s.Creator, d => d.MapFrom(x => x.CreateUser.UserName))
                .ForMember(s => s.DataCount, d => d.MapFrom(x => x.Datas.Count))
                .ForMember(s => s.SolutionCount, d => d.MapFrom(x => x.Solutions.Count))
                .ForMember(s => s.MemoryLimitMb, d => d.MapFrom(x => x.Datas.Max(m => m.MemoryLimitMb)))
                .ForMember(s => s.TimeLimit, d => d.MapFrom(x => x.Datas.Sum(m => m.TimeLimit)));
        }
    }

    public class QuestionSummaryViewModel : QuestionSummaryBasicViewModel
    {
        [Display(Name = "通过")]
        public bool Complished { get; set; }

        public bool Started { get; set; }
    }
}