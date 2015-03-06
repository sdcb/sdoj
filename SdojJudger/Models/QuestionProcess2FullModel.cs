using Newtonsoft.Json;
using System;

namespace SdojJudger.Models
{
    public class QuestionProcess2FullModel
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
