using Newtonsoft.Json;
using SdojWeb.Infrastructure.Mapping;

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
}