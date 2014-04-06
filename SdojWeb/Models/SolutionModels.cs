using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Mapping;

namespace SdojWeb.Models
{
    public class Solution
    {
        public int Id { get; set; }

        public virtual ApplicationUser CreateUser { get; set; }

        public string CreateUserId { get; set; }

        public virtual Question Question { get; set; }

        [Required]
        public int QuestionId { get; set; }

        public Languages Language { get; set; }

        [Display(Name = "代码"), Required]
        public string Source { get; set; }

        [Display(Name = "状态")]
        public SolutionStatus Status { get; set; }

        [Display(Name = "提交时间")]
        public DateTime SubmitTime { get; set; }
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