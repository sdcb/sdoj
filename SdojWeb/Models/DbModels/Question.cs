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

        // 这里必须要为可空，因为存在和QuestionData的循环引用。
        public int? SampleDataId { get; set; }

        public QuestionData SampleData { get; set; }

        public ICollection<QuestionData> Datas { get; set; }

        public ICollection<Solution> Solutions { get; set; }
    }
}