using SdojJudger.SandboxDll;

namespace SdojJudger.Runner
{
    public class JudgeResult
    {
        public int ErrorCode { get; set; }

        public int ExitCode { get; set; }

        public int TimeMs { get; set; }

        public float MemoryMb { get; set; }

        public string Output { get; set; }

        public string ErrorMessage { get; set; }

        public bool IsDone { get; set; }

        public bool Succeed
        {
            get { return IsDone && ExitCode == 0; }
        }

        public static explicit operator JudgeResult(SandboxRunResult r)
        {
            return new JudgeResult
            {
                ErrorCode = r.ErrorCode,
                ErrorMessage = r.ErrorMessage,
                ExitCode = r.ExitCode,
                MemoryMb = r.MemoryMb,
                IsDone = r.Succeed,
                TimeMs = r.TimeMs,
            };
        }

        public static explicit operator JudgeResult(SandboxIoResult r)
        {
            return new JudgeResult
            {
                ErrorCode = r.ErrorCode, 
                ErrorMessage = r.ErrorMessage, 
                IsDone = r.IsDone, 
            };
        }
    }

    public class Process2JudgeResult
    {
        public JudgeResult P1Result { get; set; }
        
        public JudgeResult P2Result { get; set; }

        public bool Succeed
        {
            get { return Process1Ok && Process2Ok; }
        }

        public bool Process1Ok
        {
            get { return TargetOk(P1Result); }
        }

        public bool Process2Ok
        {
            get { return TargetOk(P2Result); }
        }

        public bool Accepted
        {
            get
            {
                if (P1Result != null && P1Result.ExitCode == 0 && P1Result.Output == "OK")
                    return true;
                /* else */
                return false;
            }
        }

        public string Process1Output
        {
            get
            {
                if (P1Result != null && P1Result.Output != null)
                    return P1Result.Output;
                /* else */
                return null;
            }
        }

        private static bool TargetOk(JudgeResult r)
        {
            return r != null && r.IsDone && r.ErrorCode == 0;
        }
    }
}
