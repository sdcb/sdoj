using System.Runtime.InteropServices;

namespace SafeRunnerTest
{
    public static class NativeDll
    {
        [DllImport("safe_runner", EntryPoint = "cluck")]
        public extern static void Cluck();

        [DllImport("safe_runner", EntryPoint = "get3")]
        public static extern int Get3();
    }
}
