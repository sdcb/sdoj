using SdojJudger.SandboxDll;
using Xunit;

namespace SandboxTests.RunTest
{
    public class BasicRunTest
    {
        [Fact]
        [Trait("Run", "Basic")]
        public void We_can_call_BeginRun()
        {
            Sandbox.BeginRun(new SandboxRunInfo
            {
                LimitProcessCount = false, 
                MemoryLimitMb = 10.0f, 
                Path = "whoami", 
                TimeLimitMs = 1000
            });
        }
    }
}
