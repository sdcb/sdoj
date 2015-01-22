using SdojWeb.Infrastructure.Mapping;
using SdojWeb.Models.DbModels;
using System;
using System.ComponentModel;
using AutoMapper;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace SdojWeb.Models
{
    public class QuestionGroupListModel : IHaveCustomMapping
    {
        public int Id { get; set; }

        [DisplayName("名称")]
        public string Name { get; set; }

        [DisplayName("题目数量")]
        public int QuestionCount { get; set; }

        [DisplayName("创建时间")]
        public DateTime CreateTime { get; set; }

        [DisplayName("修改时间")]
        public DateTime ModifyTime { get; set; }

        public int CreateUserId { get; set; }

        [DisplayName("作者")]
        public string CreateUserName { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionGroup, QuestionGroupListModel>()
                .ForMember(d => d.QuestionCount, s => s.MapFrom(x => x.Questions.Count))
                .ForMember(d => d.CreateUserName, s => s.MapFrom(x => x.CreateUser.UserName));
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
        [MaxLength(20), Required]
        public string Name { get; set; }

        [DisplayName("描述")]
        [MaxLength(4000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public List<QuestionGroupItemEditModel> Questions { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionGroup, QuestionGroupEditModel>()
                .ForMember(s => s.Questions, d => d.MapFrom(x => x.Questions));

            configuration.CreateMap<QuestionGroupEditModel, QuestionGroup>();
        }
    }

    public class QuestionGroupItemEditModel : IHaveCustomMapping
    {
        [HiddenInput]
        public int Id { get; set; }

        [DisplayName("问题ID")]
        public string QuestionId { get; set; }

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
        }
    }

    public class QuestionGroupItemQuestionModel : IHaveCustomMapping
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Author { get; set; }

        public DateTime UpdateTime { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<Question, QuestionGroupItemQuestionModel>()
                .ForMember(s => s.Author, d => d.MapFrom(x => x.CreateUser.UserName));
        }
    }
}