using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SdojWeb.Manager
{
    public class ContestCreateModel
    {
        [Required, MaxLength(30)]
        public string Name { get; set; }

        public bool Public { get; set; }

        public TimeSpan Duration { get; set; }

        public List<int> QuestionIds { get; set; }

        public List<int> UserIds { get; set; }
    }
}