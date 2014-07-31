using Newtonsoft.Json;

namespace SdojJudger.Models
{
    public class ClientSolutionPushModel 
    {
        [JsonProperty("a")]
        public int Id { get; set; }

        [JsonProperty("b")]
        public Languages Language { get; set; }

        [JsonProperty("c")]
        public float FullMemoryLimitMb { get; set; }
    }
}
