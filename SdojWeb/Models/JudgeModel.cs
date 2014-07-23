using System.Collections.Generic;
using AutoMapper;
using SdojWeb.Infrastructure.Mapping;

namespace SdojWeb.Models
{
    public class JudgeModel : IHaveCustomMapping
    {
        public int SolutionId { get; set; }

        public int QuestionId { get; set; }

        public string Source { get; set; }

        public Languages Language { get; set; }

        public int QuestionCreateUserId { get; set; }

        public List<QuestionDataHashModel> QuestionDatas { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<Solution, JudgeModel>()
                .ForMember(s => s.SolutionId, d => d.MapFrom(x => x.Id))
                .ForMember(s => s.QuestionDatas, d => d.MapFrom(x => x.Question.Datas))
                .ForMember(s => s.QuestionCreateUserId, d => d.MapFrom(x => x.Question.CreateUserId));
        }
    }
}