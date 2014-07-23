using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SdojWeb.Infrastructure.Mapping;

namespace SdojWeb.Models
{
    public class UserSummaryViewModel : IHaveCustomMapping
    {
        public int Id { get; set; }

        [Display(Name="用户名")]
        public string UserName { get; set; }

        [Display(Name = "邮件验证")]
        public bool EmailConfirmed { get; set; }

        [Display(Name="角色")]
        public ICollection<Role> Roles { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<User, UserSummaryViewModel>()
                .ForMember(source => source.Roles, dest => dest.MapFrom(u => u.Roles));
        }
    }
}