using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using SdojWeb.Infrastructure.Mapping;
using SdojWeb.Models.DbModels;

namespace SdojWeb.Models.JudgePush
{
    public class SolutionPushModel : IHaveCustomMapping
    {
        [JsonProperty("a")]
        public int Id { get; set; }

        [JsonProperty("b")]
        public Languages Language { get; set; }

        [JsonProperty("c")]
        public float FullMemoryLimitMb { get; set; }

        [JsonProperty("d")]
        public QuestionTypes QuestionType { get; set; }

        [JsonProperty("e")]
        public int QuestionId { get; set; }

        [JsonIgnore]
        public int QuestionCreateUserId { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<Solution, SolutionPushModel>()
                .ForMember(d => d.FullMemoryLimitMb, s => s.MapFrom(x => x.Question.QuestionType == QuestionTypes.DataDrive ?
                    x.Question.Datas.Max(d => d.MemoryLimitMb) : 
                    x.Question.Process2JudgeCode.MemoryLimitMb))
                .ForMember(d => d.QuestionCreateUserId, s => s.MapFrom(x => x.Question.CreateUserId))
                .ForMember(d => d.QuestionType, s => s.MapFrom(x => x.Question.QuestionType));
        }
    }
}