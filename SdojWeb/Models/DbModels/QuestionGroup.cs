using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SdojWeb.Models.DbModels
{
    [Table("QuestionGroup")]
    public class QuestionGroup
    {
        public int Id { get; set; }

        [MaxLength(20)]
        [Index(IsUnique = true)]
        [Required]
        public string Name { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }

        [Index]
        public DateTime ModifyTime { get; set; }

        public int CreateUserId { get; set; }

        public User CreateUser { get; set; }

        public ICollection<QuestionGroupItem> Questions { get; set; }
    }

    [Table("QuestionGroupItem")]
    public class QuestionGroupItem
    {
        [Column(Order = 1)]
        [Key]
        public int QuestionGroupId { get; set; }

        [Column(Order = 2)]
        [Key]
        public int QuestionId { get; set; }

        [MaxLength(20)]
        public string QuestionName { get; set; }

        public int Order { get; set; }

        public QuestionGroup QuestionGroup { get; set; }

        public Question Question { get; set; }
    }
}