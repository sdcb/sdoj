using System;
using Newtonsoft.Json;

namespace SdojJudger.Models
{
    public class DataHashModel
    {
        [JsonProperty("a")]
        public int Id { get; set; }

        [JsonProperty("b")]
        public DateTime UpdateTime { get; set; }
    }
}
