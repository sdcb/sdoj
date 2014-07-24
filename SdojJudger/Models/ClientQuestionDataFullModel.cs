using Newtonsoft.Json;

namespace SdojJudger.Models
{
    public class ClientQuestionDataFullModel
    {
        [JsonProperty("a")]
        public int Id { get; set; }

        [JsonProperty("b")]
        public int QuestionId { get; set; }

        [JsonProperty("c")]
        public string Input { get; set; }

        [JsonProperty("d")]
        public string Output { get; set; }

        [JsonProperty("e")]
        public float MemoryLimitMb { get; set; }

        [JsonProperty("f")]
        public int TimeLimit { get; set; }
    }
}
