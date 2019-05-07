using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using YamlDotNet.RepresentationModel;

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
            using (var stream = new StreamReader(
                typeof(CodeTemplate).Assembly.GetManifestResourceStream("SdojWeb.Content.CodeTemplate.yaml")))
            {
                var yaml = new YamlStream();
                yaml.Load(stream);
                foreach (string langString in Enum.GetNames(typeof(Languages)))
                {
                    string code = yaml.Documents[0].RootNode[langString].ToString();
                    yield return new CodeTemplate
                    {
                        Language = (Languages)Enum.Parse(typeof(Languages), langString), 
                        Template = code, 
                    };
                }
            }
        }
    }
}