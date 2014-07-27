using System.Collections.Generic;
using Newtonsoft.Json;

namespace SdojJudger.Models
{
    public class ClientSolutionFullModel
    {
        [JsonProperty("a")]
        public string Source { get; set; }

        [JsonProperty("b")]
        public List<ClientQuestionDataHashModel> QuestionDatas { get; set; }
    }
}
