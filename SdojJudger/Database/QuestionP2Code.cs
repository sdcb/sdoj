using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SdojJudger.Models
{
    public class QuestionP2Code
    {
        public int QuestionId { get; set; }
        
        [Column(TypeName = "ntext"), MaxLength]
        public string Code { get; set; }
        
        public Languages Language { get; set; }
        
        public short RunTimes { get; set; }
        
        public int TimeLimitMs { get; set; }
        
        public float MemoryLimitMb { get; set; }
        
        public long UpdateTicks { get; set; }

        public static explicit operator QuestionP2Code(QuestionProcess2FullModel b)
        {
            return new QuestionP2Code
            {
                Code = b.Code, 
                Language = b.Language, 
                MemoryLimitMb = b.MemoryLimitMb, 
                QuestionId = b.QuestionId, 
                RunTimes = b.RunTimes, 
                TimeLimitMs = b.TimeLimitMs,
                UpdateTicks = b.UpdateTime.Ticks, 
            };
        }

        public static explicit operator QuestionProcess2FullModel(QuestionP2Code b)
        {
            return new QuestionProcess2FullModel
            {
                Code = b.Code, 
                Language = b.Language, 
                MemoryLimitMb = b.MemoryLimitMb, 
                QuestionId = b.QuestionId,
                RunTimes = b.RunTimes,
                TimeLimitMs = b.TimeLimitMs, 
                UpdateTime = new DateTime(b.UpdateTicks), 
            };
        }
    }
}
