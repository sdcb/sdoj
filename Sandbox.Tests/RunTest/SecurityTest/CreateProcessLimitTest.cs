using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using SdojJudger.Compiler;
using SdojJudger.SandboxDll;
using Xunit;
using Xunit.Extensions;

namespace SandboxTests.RunTest.SecurityTest
{
    [Trait("Run", "Process Limit")]
    public class CreateProcessLimitTest
    {
        [Fact]
        public void Create_process_code_should_can_be_compiled()
        {
            // Arrange
            using (var compiler = new CSharpCompiler())
            {
                // Act
                var asm = compiler.Compile(Code);

                // Assert
                asm.HasErrors.Should().BeFalse();
            }
        }

        [Theory]
        [InlineData(3, 2, true)]
        [InlineData(3, 3, false)]
        public void Create_multiple_process_in_limit_should_be_ok(int limitCount, int createCount, bool result)
        {
            // Arrange
            using (var compiler = new CSharpCompiler())
            {
                var asm = compiler.Compile(Code);

                var info = new SandboxRunInfo
                {
                    LimitProcessCount = limitCount, 
                    MemoryLimitMb = 40.0f, 
                    Path = asm.PathToAssembly, 
                    TimeLimitMs = 1000, 
                };

                var ior = Sandbox.BeginRun(info);
                using (var writer = new StreamWriter(ior.InputWriteStream))
                {
                    writer.Write(createCount);
                }

                var res = Sandbox.EndRun(ior.InstanceHandle);

                res.Succeed.Should().BeTrue();

                if (result)
                {
                    res.ExitCode.Should().Be(0);
                }
                else
                {
                    res.ExitCode.Should().NotBe(0);
                }

                res.TimeMs.Should().BeLessThan(info.TimeLimitMs);
                res.MemoryMb.Should().BeLessThan(info.MemoryLimitMb);

                Console.WriteLine("Time: {0}ms", res.TimeMs);
                Console.WriteLine("Memory: {0}MB", res.MemoryMb);
                Console.WriteLine(info.Path);
                Thread.Sleep(10);
            }
        }

        public static string Code =
            "using System;                                          " +
            "using System.Diagnostics;                              " +
            "using System.Threading;                                " +
            "                                                       " +
            "class Program                                          " +
            "{                                                      " +
            "    static void Main()                                 " +
            "    {                                                  " +
            "        var c = int.Parse(Console.ReadLine());         " +
            "        for (int i = 0; i < c; ++i)                    " +
            "        {                                              " +
            "            Process.Start(\"cmd.exe\");                " +
            "        }                                              " +
            "    }                                                  " +
            "}                                                      ";
    }
}
