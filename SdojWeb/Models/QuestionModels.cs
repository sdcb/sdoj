using System.Linq;
using System.Web;
using AutoMapper;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Infrastructure.Mapping;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SdojWeb.Models.DbModels;
using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.ModelMetadata.Attributes;
using System.Collections.Generic;

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
        public float? MemoryLimitMb { get; set; }

        [Display(Name = "总时间限制"), DisplayFormat(DataFormatString = "{0} ms")]
        public int? TimeLimit { get; set; }

        [Display(Name = "解答数")]
        public int SolutionCount { get; set; }

        [Display(Name = "通过数")]
        public int AcceptedCount { get; set; }

        [Display(Name = "题目类型")]
        public QuestionTypes QuestionType { get; set; }

        public List<QuestionDataSampleModel> Samples { get; set; }

        public int CreateUserId { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<Question, QuestionDetailModel>()
                .ForMember(s => s.Samples, d => d.MapFrom(x => x.Datas.Where(data => data.IsSample)))
				.ForMember(s => s.MemoryLimitMb, d => d.MapFrom(x => x.QuestionType == QuestionTypes.DataDrive ? x.Datas.Max(m => m.MemoryLimitMb) : x.Process2JudgeCode.MemoryLimitMb))
				.ForMember(s => s.TimeLimit, d => d.MapFrom(x => x.QuestionType == QuestionTypes.DataDrive ? x.Datas.Sum(m => m.TimeLimit) : x.Process2JudgeCode.TimeLimitMs))
                .ForMember(s => s.SolutionCount, d => d.MapFrom(x => x.Solutions.Count()))
                .ForMember(s => s.AcceptedCount, d => d.MapFrom(x => x.Solutions.Count(v => v.State == SolutionState.Accepted)));
        }
    }

    public class QuestionCreateModel : IHaveCustomMapping
    {
        public QuestionCreateModel()
        {
            TimeLimit = 1000;
            MemoryLimitMb = 64.0f;
            RunTimes = 1;
        }

        [RenderMode(RenderMode.Neither)]
        public int Id { get; set; }

        [Display(Name = "标题"), Required, MaxLength(30), Remote("CheckName", "Question", HttpMethod = "POST")]
        public string Name { get; set; }

        [Display(Name = "描述"), Required, MaxLength(4000), DataType("Markdown")]
        public string Description { get; set; }

        [Display(Name = "时间限制(ms)"), Required]
        public int TimeLimit { get; set; }

        [Display(Name = "内存限制(MB)"), Required]
        public float MemoryLimitMb { get; set; }

        [Display(Name = "输入说明"), MaxLength(1000), DataType("Markdown")]
        public string InputExplain { get; set; }

        [Display(Name = "输出说明"), MaxLength(1000), DataType("Markdown")]
        public string OutputExplain { get; set; }
        
        [Display(Name = "示例输入"), MaxLength(4000), DataType(DataType.MultilineText)]
        public string SampleInput { get; set; }

        [Display(Name = "示例输出"), Required, MaxLength(4000), DataType(DataType.MultilineText)]
        public string SampleOutput { get; set; }
        

        [Display(Name = "题目类型")]
        public QuestionTypes QuestionType { get; set; }

        // 仅在QuestionType = Process2Drive时显示。
        [Display(Name = "语言")]
        public Languages Language { get; set; }

        // 仅在QuestionType = Process2Drive时显示。
        [Display(Name = "运行次数", Prompt = "请输入1~10以内的整数。"), Range(1, 10)]
        public short RunTimes { get; set; }

        // 仅在QuestionType = Process2Drive时显示。
        [Display(Name = "代码"), Required, DataType(DataType.MultilineText)]
        public string Source { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionCreateModel, Question>()
                .ForMember(s => s.CreateTime, d => d.MapFrom(x => DateTime.Now))
                .ForMember(s => s.CreateUserId, d => d.MapFrom(x => HttpContext.Current.User.Identity.GetUserId<int>()))
                .ForMember(s => s.UpdateTime, d => d.MapFrom(x => DateTime.Now))
                .ForMember(s => s.Datas, d => d.MapFrom(x => new[] { Mapper.Map<QuestionData>(x) }));
            configuration.CreateMap<QuestionCreateModel, QuestionData>()
                .ForMember(s => s.Input, d => d.MapFrom(x => x.SampleInput))
                .ForMember(s => s.Output, d => d.MapFrom(x => x.SampleOutput))
                .ForMember(s => s.MemoryLimitMb, d => d.MapFrom(x => x.MemoryLimitMb))
                .ForMember(s => s.TimeLimit, d => d.MapFrom(x => x.TimeLimit))
                .ForMember(s => s.UpdateTime, d => d.MapFrom(x => DateTime.Now))
                .ForMember(s => s.IsSample, d => d.UseValue(true));
        }
    }

    public class QuestionEditModel : IHaveCustomMapping
    {
        [Editable(false)]
        public int Id { get; set; }

        [Display(Name = "内存限制(MB)"), Editable(false)]
        public float? MemoryLimitMb { get; set; }

        [Display(Name = "时间限制(ms)"), Editable(false)]
        public int? TimeLimit { get; set; }

        [Display(Name = "标题"), Required, MaxLength(30), Remote("CheckName", "Question", AdditionalFields = "Id", HttpMethod = "POST")]
        public string Name { get; set; }

        [Display(Name = "描述"), Required, MaxLength(4000), DataType("Markdown")]
        public string Description { get; set; }        

        [HiddenInput]
        public int? QuestionDataId { get; set; }

        [Display(Name = "输入说明"), MaxLength(1000), DataType("Markdown")]
        public string InputExplain { get; set; }

        [Display(Name = "输出说明"), MaxLength(1000), DataType("Markdown")]
        public string OutputExplain { get; set; }

		[Display(Name = "题目类型")]
		public QuestionTypes QuestionType { get; set; }

		[HiddenInput]
        public int CreateUserId { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionEditModel, Question>()
                .ForMember(source => source.UpdateTime, dest => dest.MapFrom(x => DateTime.Now));
            configuration.CreateMap<Question, QuestionEditModel>()
                .ForMember(source => source.CreateUserId, dest => dest.MapFrom(x => x.CreateUserId))
				.ForMember(s => s.MemoryLimitMb, d => d.MapFrom(x => x.QuestionType == QuestionTypes.DataDrive ? x.Datas.Max(m => m.MemoryLimitMb) : x.Process2JudgeCode.MemoryLimitMb))
				.ForMember(s => s.TimeLimit, d => d.MapFrom(x => x.QuestionType == QuestionTypes.DataDrive ? x.Datas.Sum(m => m.TimeLimit) : x.Process2JudgeCode.TimeLimitMs)); 
        }
    }

    public class QuestionNotMappedEditModel : IMapFrom<Question>
    {
        public int CreateUserId { get; set; }

        public DateTime CreateTime { get; set; }

        public int? SampleDataId { get; set; }

        public QuestionData SampleData { get; set; }
    }

    public class QuestionSummaryViewModel : IHaveCustomMapping
    {
        [HiddenInput]
        public int Id { get; set; }

        [Display(Name = "标题")]
        public string Name { get; set; }

        [Display(Name = "作者")]
        public string Creator { get; set; }

        [Display(Name = "类型")]
        public QuestionTypes QuestionType { get; set; }

        [Display(Name = "更新时间")]
        public DateTime UpdateTime { get; set; }

        [Display(Name = "测试数据"), DisplayFormat(DataFormatString = "{0}个")]
        public int DataCount { get; set; }

        [Display(Name = "通过/解答")]
        public int SolutionCount { get; set; }

        public int AcceptedCount { get; set; }

        [Display(Name = "内存限制(MB)")]
        public float? MemoryLimitMb { get; set; }

        [Display(Name = "时间限制(ms)")]
        public int? TimeLimit { get; set; }

        [Display(Name = "通过")]
        public bool Complished { get; set; }

        public bool Started { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            var currentUserId = 0;

            configuration.CreateMap<Question, QuestionSummaryViewModel>()
                .ForMember(s => s.Complished, d => d.MapFrom(x => x.Solutions.Any(s => s.CreateUserId == currentUserId && s.State == SolutionState.Accepted)))
                .ForMember(s => s.Started, d => d.MapFrom(x => x.Solutions.Any(s => s.CreateUserId == currentUserId)))
                .ForMember(s => s.Creator, d => d.MapFrom(x => x.CreateUser.UserName))
                .ForMember(s => s.DataCount, d => d.MapFrom(x => x.Datas.Count))
                .ForMember(s => s.SolutionCount, d => d.MapFrom(x => x.Solutions.Count))
                .ForMember(s => s.AcceptedCount, d => d.MapFrom(x => x.Solutions.Where(s => s.State == SolutionState.Accepted).Count()))
                .ForMember(s => s.MemoryLimitMb, d => d.MapFrom(x => x.QuestionType == QuestionTypes.DataDrive ? x.Datas.Max(m => m.MemoryLimitMb) : x.Process2JudgeCode.MemoryLimitMb))
                .ForMember(s => s.TimeLimit, d => d.MapFrom(x => x.QuestionType == QuestionTypes.DataDrive ? x.Datas.Sum(m => m.TimeLimit) : x.Process2JudgeCode.TimeLimitMs));
        }
    }

    public class QuestionProcess2CodeEditModel : IMapFrom<Process2JudgeCode>
    {
        [HiddenInput]
        public int QuestionId { get; set; }

        [Editable(false)]
        [Display(Name = "题目名")]
        public string QuestionName { get; set; }

        [Display(Name = "语言")]
        public Languages Language { get; set; }

        [Display(Name = "代码"), Required, DataType(DataType.MultilineText)]
        public string Code { get; set; }

        [Display(Name = "运行次数"), Range(1, 10)]
        public short RunTimes { get; set; }

        [Display(Name = "时间限制(ms)"), Range(1, 10000)]
        public int TimeLimitMs { get; set; }

        [Display(Name = "内存限制(MB)"), Range(0.01, 1024)]
        public float MemoryLimitMb { get; set; }
    }
}