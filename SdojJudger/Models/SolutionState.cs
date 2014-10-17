using System.ComponentModel.DataAnnotations;

namespace SdojJudger.Models
{
    public enum SolutionState
    {
        [Display(Name = "排队中")]
        Queuing = 0,
        [Display(Name = "编译中")]
        Compiling = 1,
        [Display(Name = "评测中")]
        Judging = 2, 
        [Display(Name = "已完成")]
        Completed = 100,
        [Display(Name = "编译失败")]
        CompileError = 101,
        [Display(Name = "通过")]
        Accepted = 102,
        [Display(Name = "答案错误")]
        WrongAnswer = 103,
        [Display(Name = "运行时错误")]
        RuntimeError = 104,
        [Display(Name = "超时")]
        TimeLimitExceed = 105,
        [Display(Name = "超内存")]
        MemoryLimitExceed = 106,
    }
}
