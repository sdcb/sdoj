using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SdojJudger.Database
{
    public class QuestionData
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Column(TypeName = "ntext"), MaxLength]
        public string Input { get; set; }

        [Required, Column(TypeName = "ntext"), MaxLength]
        public string Output { get; set; }

        public float MemoryLimitMb { get; set; }

        public int TimeLimit { get; set; }

        public long UpdateTicks { get; set; }
    }
}
