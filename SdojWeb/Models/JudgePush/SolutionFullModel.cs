using System.Collections.Generic;
using AutoMapper;
using Newtonsoft.Json;
using SdojWeb.Infrastructure.Mapping;
using SdojWeb.Models.DbModels;

namespace SdojWeb.Models
{
    public class SolutionFullModel : IHaveCustomMapping
    {
        [JsonProperty("a")]
        public string Source { get; set; }

        [JsonProperty("b")]
        public List<QuestionDataHashModel> QuestionDatas { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<Solution, SolutionFullModel>()
                .ForMember(s => s.QuestionDatas, d => d.MapFrom(x => x.Question.Datas));
        }
    }
}