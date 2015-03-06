using Newtonsoft.Json;

namespace SdojJudger.Models
{
    public class SolutionProcess2CodeModel
    {
        [JsonProperty("a")]
        public string Source { get; set; }

        [JsonProperty("b")]
        public DataHashModel JudgeCode { get; set; }
    }
}
