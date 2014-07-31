using SdojJudger.Models;

namespace SdojJudger.Database
{
    public class SolutionEntity
    {
        public int Id { get; set; }

        public Languages Language { get; set; }

        public float FullMemoryLimitMb { get; set; }
    }
}
