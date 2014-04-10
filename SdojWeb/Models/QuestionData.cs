using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SdojWeb.Models
{
    public class QuestionData
    {
        public int Id { get; set; }

        public virtual Question Question { get; set; }

        [Required]
        public int QuestionId { get; set; }

        public string Input { get; set; }

        [Required]
        public string Output { get; set; }

        [Required]
        public DateTime UpdateTime { get; set; }
    }
}