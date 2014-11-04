namespace SdojJudger.SandboxDll
{
    public class SandboxRunResult
    {
        public bool Succeed { get; set; }

        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public int TimeMs { get; set; }

        public float MemoryMb { get; set; }

        public int ExitCode { get; set; }
    }
}
