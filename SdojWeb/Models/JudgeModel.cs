using System.Collections.Generic;
using System.Linq;
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

        public int TimeLimit { get; set; }

        public long MemoryLimit { get; set; }

        public int QuestionCreateUserId { get; set; }

        public List<QuestionDataHashModel> QuestionDatas { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<Solution, JudgeModel>()
                .ForMember(s => s.TimeLimit, d => d.MapFrom(x => x.Question.Datas.Sum(v => v.TimeLimit)))
                .ForMember(s => s.MemoryLimit, d => d.MapFrom(x => x.Question.Datas.Max(v => v.MemoryLimitMb)))
                .ForMember(s => s.SolutionId, d => d.MapFrom(x => x.Id))
                .ForMember(s => s.QuestionDatas, d => d.MapFrom(x => x.Question.Datas))
                .ForMember(s => s.QuestionCreateUserId, d => d.MapFrom(x => x.Question.CreateUserId));
        }
    }
}