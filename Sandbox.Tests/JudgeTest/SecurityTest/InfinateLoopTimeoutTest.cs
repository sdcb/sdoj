using System.CodeDom.Compiler;
using System.Diagnostics;
using FluentAssertions;
using Microsoft.CSharp;
using SdojJudger.Runner;
using Xunit;

namespace SandboxTests.JudgeTest.SecurityTest
{
    public class InfinateLoopTimeoutTest
    {
        [Fact]
        public void InfinateLoopSource_can_be_compiled()
        {
            // Arrange
            var compiler = new CSharpCodeProvider();
            var options = new CompilerParameters
            {
                GenerateExecutable = true
            };

            // Act
            var asm = compiler.CompileAssemblyFromSource(options, InfinateLoopSource);

            // Assert
            asm.Errors.HasErrors.Should().BeFalse();
        }

        [Fact]
        public void Regular_run_InfinateLoop_should_never_stop()
        {
            // Arrange
            var compiler = new CSharpCodeProvider();
            var options = new CompilerParameters
            {
                GenerateExecutable = true
            };
            var asm = compiler.CompileAssemblyFromSource(options, InfinateLoopSource);

            // Act
            var ps = Process.Start(asm.PathToAssembly);
            var ok = ps.WaitForExit(150);

            // Assert
            ok.Should().BeFalse();

            // Clean up
            ps.Kill();
        }

        [Fact]
        public void Judge_run_InfinateLoop_should_exit()
        {
            // Arrange
            var compiler = new CSharpCodeProvider();
            var options = new CompilerParameters
            {
                GenerateExecutable = true
            };
            var asm = compiler.CompileAssemblyFromSource(options, InfinateLoopSource);
            var ji = new JudgeInfo
            {
                Input = null, 
                MemoryLimitMb = 10.0f, 
                Path = asm.PathToAssembly, 
                TimeLimitMs = 100
            };

            // Act
            var result = NativeDll.Judge(ji);

            // Assert
            result.Succeed.Should().BeTrue();
            result.ExitCode.Should().NotBe(0);
            result.TimeMs.Should().BeGreaterOrEqualTo(100);
        }

        public const string InfinateLoopSource = "class Program" +
                                                 "{" +
                                                 "    static void Main(string[] args)" +
                                                 "    {" +
                                                 "        while (true)" +
                                                 "        {" +
                                                 "        }" +
                                                 "    }" +
                                                 "}";
    }
}
