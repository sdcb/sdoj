using System;
using Newtonsoft.Json;

namespace SdojJudger.Models
{
    public class ClientQuestionDataHashModel
    {
        [JsonProperty("a")]
        public int Id { get; set; }

        [JsonProperty("b")]
        public DateTime UpdateTime { get; set; }
    }
}
