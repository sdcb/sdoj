using SdojWeb.Infrastructure.Mapping;
using SdojWeb.Models.DbModels;
using System;
using System.ComponentModel;
using AutoMapper;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using SdojWeb.Infrastructure.ModelMetadata.Attributes;
using System.Web;
using Microsoft.AspNet.Identity;

namespace SdojWeb.Models
{
    public class QuestionGroupListModel : IHaveCustomMapping
    {
        public int Id { get; set; }

        [DisplayName("名称")]
        public string Name { get; set; }

        [DisplayName("题目数")]
        public int QuestionCount { get; set; }

        [DisplayName("修改时间")]
        public DateTime ModifyTime { get; set; }

        public int CreateUserId { get; set; }

        [DisplayName("作者")]
        public string CreateUserName { get; set; }

        [DisplayName("进行数")]
        public int InProgressCount { get; set; }

        [DisplayName("完成数")]
        public int ComplishedCount { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            var currentUserId = 0;

            configuration.CreateMap<QuestionGroup, QuestionGroupListModel>()
                .ForMember(d => d.QuestionCount, s => s.MapFrom(x => x.Questions.Count))
                .ForMember(d => d.CreateUserName, s => s.MapFrom(x => x.CreateUser.UserName))
                .ForMember(d => d.InProgressCount, s => s.MapFrom(
                    x => x.Questions.Count(q => q.Question.Solutions.Any(a => a.CreateUserId == currentUserId))))
                .ForMember(d => d.ComplishedCount, s => s.MapFrom(
                    x => x.Questions.Count(q => q.Question.Solutions.Any(a => a.CreateUserId == currentUserId && a.State == SolutionState.Accepted))));
        }
    }

    public class QuestionGroupEditModel : IHaveCustomMapping
    {
        public QuestionGroupEditModel()
        {
            Questions = new List<QuestionGroupItemEditModel>();
        }

        [HiddenInput]
        public int Id { get; set; }

        [DisplayName("名称")]
        [MaxLength(20), Required, Remote("CheckName", "QuestionGroup", HttpMethod = "POST", AdditionalFields = "Id")]
        public string Name { get; set; }

        [DisplayName("描述")]
        [MaxLength(4000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [RenderMode(RenderMode.Neither)]
        public List<QuestionGroupItemEditModel> Questions { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionGroup, QuestionGroupEditModel>()
                .ForMember(s => s.Questions, d => d.MapFrom(x => x.Questions));

            configuration.CreateMap<QuestionGroupEditModel, QuestionGroup>()
                .ForMember(s => s.ModifyTime, d => d.MapFrom(x => DateTime.Now))
                .ForMember(s => s.CreateUserId, d => d.MapFrom(x => HttpContext.Current.User.Identity.GetUserId<int>()));
        }
    }

    public class QuestionGroupItemEditModel : IHaveCustomMapping
    {
        [HiddenInput]
        public int? Id { get; set; }

        [DisplayName("问题ID")]
        public int QuestionId { get; set; }

        [MaxLength(20)]
        [DisplayName("别名")]
        public string Alias { get; set; }

        [DisplayName("题目名")]
        public string Name { get; set; }

        [DisplayName("作者")]
        public string Author { get; set; }

        [DisplayName("顺序")]
        public int Order { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionGroupItemEditModel, QuestionGroupItem>()
                .ForMember(s => s.QuestionGroupId, d => d.MapFrom(x => x.Id ?? 0))
                .ForMember(s => s.QuestionName, d => d.MapFrom(x => x.Alias));

            configuration.CreateMap<QuestionGroupItem, QuestionGroupItemEditModel>()
                .ForMember(s => s.Id, d => d.MapFrom(x => x.QuestionGroupId))
                .ForMember(s => s.Alias, d => d.MapFrom(x => x.QuestionName))
                .ForMember(s => s.Name, d => d.MapFrom(x => x.Question.Name))
                .ForMember(s => s.Author, d => d.MapFrom(x => x.Question.CreateUser.UserName));
        }
    }

    public class QuestionGroupDetailModel : IMapFrom<QuestionGroup>, IHaveCustomMapping
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int CreateUserId { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionGroupDetailModel, QuestionGroup>()
                .ForMember(s => s.ModifyTime, d => d.MapFrom(x => DateTime.Now))
                .ForMember(s => s.CreateUserId, d => d.MapFrom(x => HttpContext.Current.User.Identity.GetUserId<int>()));
        }
    }

    public class QuestionGroupDetailItemModel : IHaveCustomMapping
    {
        [DisplayName("别名")]
        public string Alias { get; set; }

        public int Order { get; set; }

        public QuestionSummaryViewModel Question { get; set; }
        
        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionGroupItem, QuestionGroupDetailItemModel>()
                .ForMember(s => s.Alias, s => s.MapFrom(x => x.QuestionName))
                .ForMember(s => s.Order, s => s.MapFrom(x => x.Order))
                .ForMember(s => s.Question, s => s.MapFrom(x => x.Question));
        }
    }
}