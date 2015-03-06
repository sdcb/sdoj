using System.Collections.Generic;
using Newtonsoft.Json;

namespace SdojJudger.Models
{
    public class SolutionFullModel
    {
        [JsonProperty("a")]
        public string Source { get; set; }

        [JsonProperty("b")]
        public List<DataHashModel> QuestionDatas { get; set; }
    }
}
