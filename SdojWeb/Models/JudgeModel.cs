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

        public int TimeLimit { get; set; }

        public long MemoryLimit { get; set; }

        public List<QuestionDataHashModel> QuestionDatas { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<Solution, JudgeModel>()
                .ForMember(source => source.TimeLimit, dest => dest.MapFrom(x => x.Question.TimeLimit))
                .ForMember(source => source.MemoryLimit, dest => dest.MapFrom(x => (long) (x.Question.MemoryLimitMb*1024*1024)))
                .ForMember(source => source.SolutionId, dest => dest.MapFrom(x => x.Id))
                .ForMember(source => source.QuestionDatas, dest => dest.MapFrom(x => x.Question.Datas));
        }
    }
}