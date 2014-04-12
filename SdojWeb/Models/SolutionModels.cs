using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using AutoMapper;
using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Mapping;

namespace SdojWeb.Models
{
    public class Solution : IHaveCustomMapping
    {
        public int Id { get; set; }

        public virtual ApplicationUser CreateUser { get; set; }

        public string CreateUserId { get; set; }

        public virtual Question Question { get; set; }

        [Required]
        public int QuestionId { get; set; }

        public Languages Language { get; set; }

        [Required]
        public string Source { get; set; }

        public SolutionStatus Status { get; set; }

        [Required]
        public float UsingMemoryMb { get; set; }

        [Required]
        public int RunTime { get; set; }

        [Display(Name = "提交时间")]
        public DateTime SubmitTime { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<Solution, SolutionSummaryModel>()
                .ForMember(dest => dest.CreateUserName, source => source.MapFrom(x => x.CreateUser.UserName))
                .ForMember(dest => dest.SourceLength, source => source.MapFrom(x => x.Source.Length));
        }
    }

    public class SolutionCreateModel : IHaveCustomMapping
    {
        [Display(Name = "题目"), Required]
        public int QuestionId { get; set; }

        [Display(Name = "语言"), Required]
        public Languages Language { get; set; }

        [Display(Name = "源代码"), Required, DataType(DataType.MultilineText)]
        public string Source { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<SolutionCreateModel, Solution>()
                .ForMember(dest => dest.SubmitTime, source => source.MapFrom(x => DateTime.Now))
                .ForMember(dest => dest.CreateUserId, source => source.MapFrom(x => HttpContext.Current.User.Identity.GetUserId()))
                .ForMember(dest => dest.Status, source => source.MapFrom(x => SolutionStatus.Queuing))
                .ForMember(dest => dest.QuestionId, source => source.MapFrom(x => x.QuestionId));
        }
    }

    public class SolutionDetailModel : IMapFrom<Solution>
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }

        [Display(Name = "题目")]
        public string QuestionName { get; set; }

        [Display(Name = "语言")]
        public Languages Language { get; set; }

        [Display(Name = "源代码")]
        public string Source { get; set; }
    }

    public class SolutionDeleteModel : IMapFrom<Solution>
    {
        public int Id { get; set; }

        public string CreateUserId { get; set; }

        public int QuestionId { get; set; }

        [Display(Name = "题目")]
        public string QuestionName { get; set; }

        [Display(Name = "源代码")]
        public string Source { get; set; }
    }

    public class SolutionSummaryModel
    {
        public int Id { get; set; }

        [Display(Name = "用户名")]
        public string CreateUserName { get; set; }

        public string CreateUserId { get; set; }

        [Display(Name = "题目名")]
        public string QuestionName { get; set; }

        public int QuestionId { get; set; }

        [Display(Name = "语言")]
        public Languages Language { get; set; }

        [Display(Name = "代码长度")]
        public int SourceLength { get; set; }

        [Display(Name = "状态")]
        public SolutionStatus Status { get; set; }

        [Display(Name = "内存使用(MB)")]
        public float UsingMemoryMb { get; set; }

        [Display(Name = "耗时(ms)")]
        public int RunTime { get; set; }

        [Display(Name = "提交时间")]
        public DateTime SubmitTime { get; set; }
    }

    public enum SolutionStatus
    {
        [Display(Name = "排队中")]
        Queuing, 
        Juding, 
        Accepted, 
        WrongAnswer, 
    }

    public enum Languages
    {
        [Display(Name = "C#")]
        CSharp,
        [Display(Name = "Visual Basic")]
        VB, 
        [Display(Name = "C++")]
        Cpp, 
    }
}