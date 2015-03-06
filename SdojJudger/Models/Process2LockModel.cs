using Newtonsoft.Json;

namespace SdojJudger.Models
{
    public class Process2LockModel
    {
        [JsonProperty("a")]
        public string Source { get; set; }

        [JsonProperty("b")]
        public DataHashModel JudgeCode { get; set; }
    }
}
