using System;
using System.Collections.Generic;
using AutoMapper;
using Newtonsoft.Json;
using SdojWeb.Infrastructure.Mapping;
using SdojWeb.Models.DbModels;

namespace SdojWeb.Models
{
    public class SolutionDataModel : IHaveCustomMapping
    {
        [JsonProperty("a")]
        public string Source { get; set; }

        [JsonProperty("b")]
        public List<DataHashModel> QuestionDatas { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<Solution, SolutionDataModel>()
                .ForMember(s => s.QuestionDatas, d => d.MapFrom(x => x.Question.Datas));
        }
    }

    public class SolutionProcess2CodeModel : IHaveCustomMapping
    {
        [JsonProperty("a")]
        public string Source { get; set; }

        [JsonProperty("b")]
        public DataHashModel JudgeCode { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<Solution, SolutionProcess2CodeModel>()
                .ForMember(s => s.JudgeCode, d => d.MapFrom(x => x.Question.Process2JudgeCode));
        }
    }
}