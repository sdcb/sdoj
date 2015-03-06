using Newtonsoft.Json;

namespace SdojJudger.Models
{
    public class SolutionPushModel 
    {
        [JsonProperty("a")]
        public int Id { get; set; }

        [JsonProperty("b")]
        public Languages Language { get; set; }

        [JsonProperty("c")]
        public float FullMemoryLimitMb { get; set; }

        [JsonProperty("d")]
        public QuestionTypes QuestionType { get; set; }
    }
}
