using System.Collections.Generic;
using AutoMapper;
using Newtonsoft.Json;
using SdojWeb.Infrastructure.Mapping;

namespace SdojWeb.Models
{
    public class JudgeModel : IHaveCustomMapping
    {
        [JsonProperty("a")]
        public int SolutionId { get; set; }

        [JsonProperty("b")]
        public int QuestionId { get; set; }

        [JsonProperty("c")]
        public string Source { get; set; }

        [JsonProperty("d")]
        public Languages Language { get; set; }

        [JsonProperty("e")]
        public int QuestionCreateUserId { get; set; }

        [JsonProperty("f")]
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