using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using SdojWeb.Infrastructure.Mapping;
using SdojWeb.Models.DbModels;
using Microsoft.AspNet.Identity;

namespace SdojWeb.Models
{
    public class SolutionCreateModel : IHaveCustomMapping
    {
        [Display(Name = "ID"), Required, HiddenInput]
        public int QuestionId { get; set; }

        [Display(Name = "题目"), Editable(false)]
        public string Name { get; set; }

        [Display(Name = "语言"), Required]
        public Languages Language { get; set; }

        [Display(Name = "源代码"), Required, DataType("Code")]
        [MaxLength(32*1024)]
        public string Source { get; set; }

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<SolutionCreateModel, Solution>()
                .ForMember(dest => dest.SubmitTime, source => source.MapFrom(x => DateTime.Now))
                .ForMember(dest => dest.CreateUserId, source => source.MapFrom(x => HttpContext.Current.User.Identity.GetUserId<int>()))
                .ForMember(dest => dest.State, source => source.MapFrom(x => SolutionState.Queuing))
                .ForMember(dest => dest.QuestionId, source => source.MapFrom(x => x.QuestionId));
        }
    }

    public class SolutionDetailModel : IMapFrom<Solution>
    {
        public int Id { get; set; }

        public int CreateUserId { get; set; }

        public int QuestionId { get; set; }

        public int QuestionCreateUserId { get; set; }

        [Display(Name = "题目")]
        public string QuestionName { get; set; }

        [Display(Name = "语言")]
        public Languages Language { get; set; }

        [Display(Name = "源代码")]
        public string Source { get; set; }
    }

    public class SolutionSummaryModel : IHaveCustomMapping
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "用户名")]
        public string CreateUserName { get; set; }

        public int CreateUserId { get; set; }

        public int QuestionCreateUserId { get; set; }

        [Display(Name = "题目名")]
        public string QuestionName { get; set; }

        public int QuestionId { get; set; }

        [Display(Name = "语言")]
        public Languages Language { get; set; }

        [Display(Name = "代码长度")]
        public int SourceLength { get; set; }

        [Display(Name = "状态")]
        public SolutionState State { get; set; }

        [Display(Name = "内存使用(MB)"), DisplayFormat(DataFormatString = "{0:F2}")]
        public float UsingMemoryMb { get; set; }

        [Display(Name = "耗时(ms)")]
        public int RunTime { get; set; }

        [Display(Name = "提交时间")]
        public DateTime SubmitTime { get; set; }

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<Solution, SolutionSummaryModel>()
                .ForMember(dest => dest.CreateUserName, source => source.MapFrom(x => x.CreateUser.UserName))
                .ForMember(dest => dest.SourceLength, source => source.MapFrom(x => x.Source.Length));
        }
    }

    public enum SolutionState
    {
        [Display(Name = "排队中")]
        Queuing = 0,
        [Display(Name = "编译中")]
        Compiling = 1,
        [Display(Name = "评测中")]
        Judging = 2,
        [Display(Name = "已完成")]
        Completed = 100,
        [Display(Name = "编译失败")]
        CompileError = 101,
        [Display(Name = "通过")]
        Accepted = 102,
        [Display(Name = "答案错误")]
        WrongAnswer = 103,
        [Display(Name = "运行时错误")]
        RuntimeError = 104,
        [Display(Name = "超时")]
        TimeLimitExceed = 105,
        [Display(Name = "超内存")]
        MemoryLimitExceed = 106, 
    }

    public enum Languages
    {
        [Display(Name = "C# 7.3")]
        CSharp = 1,
        [Display(Name = "Visual Basic 16.0")]
        Vb = 2, 
        [Display(Name = "MSVC C++ 19.20.27508.1")]
        Cpp = 3,
        [Display(Name = "MSVC C 19.20.27508.1")]
        C = 4, 
        [Display(Name = "Python 3.7")]
        Python3 = 5, 
        [Display(Name = "Java 12")]
        Java = 6, 
    }
}