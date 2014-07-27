using Newtonsoft.Json;

namespace SdojJudger.Models
{
    public class ClientSolutionPushModel 
    {
        [JsonProperty("a")]
        public int SolutionId { get; set; }

        [JsonProperty("b")]
        public Languages Language { get; set; }

        [JsonProperty("c")]
        public float FullMemoryLimitMb { get; set; }

        [JsonIgnore]
        public int QuestionCreateUserId { get; set; }
    }
}
