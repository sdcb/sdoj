using Newtonsoft.Json;
using SdojWeb.Infrastructure.Mapping;
using SdojWeb.Models.DbModels;
using System;

namespace SdojWeb.Models
{
    public class QuestionDataFullModel : IMapFrom<QuestionData>
    {
        [JsonProperty("a")]
        public int Id { get; set; }

        [JsonProperty("b")]
        public string Input { get; set; }

        [JsonProperty("c")]
        public string Output { get; set; }

        [JsonProperty("d")]
        public float MemoryLimitMb { get; set; }

        [JsonProperty("e")]
        public int TimeLimit { get; set; }
    }

    public class QuestionProcess2CodeFullModel : IMapFrom<Process2JudgeCode>
    {
        [JsonProperty("a")]
        public int QuestionId { get; set; }

        [JsonProperty("b")]
        public string Code { get; set; }

        [JsonProperty("c")]
        public Languages Language { get; set; }
        
        [JsonProperty("d")]
        public short RunTimes { get; set; }

        [JsonProperty("e")]
        public int TimeLimitMs { get; set; }

        [JsonProperty("f")]
        public float MemoryLimitMb { get; set; }

        [JsonProperty("g")]
        public DateTime UpdateTime { get; set; }
    }
}