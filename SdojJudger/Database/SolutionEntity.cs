using System.ComponentModel.DataAnnotations.Schema;
using SdojJudger.Models;

namespace SdojJudger.Database
{
    public class SolutionEntity
    {
        public int Id { get; set; }

        public Languages Language { get; set; }

        [Column(TypeName = "ntext")]
        public string Source { get; set; }
    }
}
