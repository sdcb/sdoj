using SdojWeb.Infrastructure.Mapping;
using SdojWeb.Models.DbModels;
using System;
using System.ComponentModel;
using AutoMapper;

namespace SdojWeb.Models
{
    public class QuestionGroupListModel : IHaveCustomMapping
    {
        public int Id { get; set; }

        [DisplayName("名称")]
        public string Name { get; set; }

        [DisplayName("题目数")]
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
}