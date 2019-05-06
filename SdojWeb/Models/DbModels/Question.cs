using SdojWeb.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SdojWeb.Models.DbModels
{
    public class Question : IMapFrom<QuestionNotMappedEditModel>
    {
        public int Id { get; set; }

        public int CreateUserId { get; set; }

        public User CreateUser { get; set; }

        [Required, MaxLength(30), Index(IsUnique = true)]
        public string Name { get; set; }

        [Required, MaxLength(4000)]
        public string Description { get; set; }

        [MaxLength(1000)]
        public string InputExplain { get; set; }

        [MaxLength(1000)]
        public string OutputExplain { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public QuestionTypes QuestionType { get; set; }

        public ICollection<QuestionData> Datas { get; set; }

        public ICollection<Solution> Solutions { get; set; }

        public ICollection<QuestionGroupItem> QuestionGroups { get; set; }

        public ICollection<ContestQuestion> Contests { get; set; }

        public ICollection<QuestionCodeTemplate> CodeTemplates { get; set; }

        public Process2JudgeCode Process2JudgeCode { get; set; }
    }

    public enum QuestionTypes : byte
    {
        [Display(Name = "数据驱动")]
        DataDrive = 0, 

        [Display(Name = "第二进程驱动")]
        Process2Drive = 1, 
    }
}