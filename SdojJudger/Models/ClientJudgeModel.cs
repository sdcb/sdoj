using System.Collections.Generic;
using Newtonsoft.Json;

namespace SdojJudger.Models
{
    public class ClientJudgeModel
    {
        [JsonProperty("a")]
        public int SolutionId { get; set; }

        [JsonProperty("b")]
        public int QuestionId { get; set; }

        [JsonProperty("c")]
        public string Source { get; set; }

        [JsonProperty("d")]
        public Languages Language { get; set; }

        [JsonProperty("e")]
        public int QuestionCreateUserId { get; set; }

        [JsonProperty("f")]
        public List<ClientQuestionDataHashModel> QuestionDatas { get; set; }
    }
}
