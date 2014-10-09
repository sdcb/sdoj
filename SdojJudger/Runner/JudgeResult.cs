using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SdojJudger.Runner
{
    public class JudgeResult
    {
        public int ErrorCode { get; set; }

        public int ExceptionCode { get; set; }

        public int ExitCode { get; set; }

        public int TimeMs { get; set; }

        public float MemoryMb { get; set; }

        public string Output { get; set; }

        public string ErrorMessage { get; set; }

        public string ExceptionMessage { get; set; }

        public bool Succeed { get; set; }
    }
}
