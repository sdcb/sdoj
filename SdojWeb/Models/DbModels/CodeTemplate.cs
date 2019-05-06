using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SdojWeb.Models.DbModels
{
    public class QuestionCodeTemplate
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }

        public Question Question { get; set; }

        public Languages Language { get; set; }

        public string Template { get; set; }
    }

    public class CodeTemplate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Languages Language { get; set; }

        public string Template { get; set; }

        public static IEnumerable<CodeTemplate> GetDefaultTemplates()
        {
            throw new NotImplementedException();
        }
    }
}