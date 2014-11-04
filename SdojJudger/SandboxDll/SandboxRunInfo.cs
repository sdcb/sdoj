namespace SdojJudger.SandboxDll
{
    public class SandboxRunInfo
    {
        public string Path { get; set; }

        public int TimeLimitMs { get; set; }

        public float MemoryLimitMb { get; set; }

        public int LimitProcessCount { get; set; }
    }
}
