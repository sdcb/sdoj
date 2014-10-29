using System.CodeDom.Compiler;
using System.Diagnostics;
using FluentAssertions;
using Microsoft.CSharp;
using SdojJudger.Runner;
using Xunit;

namespace Sandbox.Tests.SecurityTest
{
    public class MemoryLimitTest
    {
        [Fact]
        public void MemoryLimitTestSource_can_be_compiled()
        {
            // Arrange
            var compiler = new CSharpCodeProvider();
            var options = new CompilerParameters
            {
                GenerateExecutable = true
            };

            // Act
            var asm = compiler.CompileAssemblyFromSource(options, MemoryLimitTestSource);

            // Assert
            asm.Errors.HasErrors.Should().BeFalse();
        }

        [Fact]
        public void Regular_run_MemoryLimitTest_should_be_ok()
        {
            // Arrange
            var compiler = new CSharpCodeProvider();
            var options = new CompilerParameters
            {
                GenerateExecutable = true
            };
            var asm = compiler.CompileAssemblyFromSource(options, MemoryLimitTestSource);

            // Act & Assert
            var ps = Process.Start(asm.PathToAssembly);
            ps.Should().NotBeNull();

            var ok = ps.WaitForExit(200);
            ok.Should().BeTrue();
        }

        [Fact]
        public void Judge_run_MemoryLimitTest_should_be_limited()
        {
            // Arrange
            var compiler = new CSharpCodeProvider();
            var options = new CompilerParameters
            {
                GenerateExecutable = true
            };
            var asm = compiler.CompileAssemblyFromSource(options, MemoryLimitTestSource);
            var info = new JudgeInfo
            {
                Input = null, 
                MemoryLimitMb = 10.0f, 
                Path = asm.PathToAssembly, 
                TimeLimitMs = 200
            };

            // Act
            var result = NativeDll.Judge(info);

            // Assert
            result.Succeed.Should().BeTrue();
            result.ExitCode.Should().NotBe(0);
            result.MemoryMb.Should().BeGreaterOrEqualTo(20.0f);
        }

        public const string MemoryLimitTestSource = "using System;" +
                                                    "class Program" +
                                                    "{" +
                                                    "    static void Main(string[] args)" +
                                                    "    {" +
                                                    "        var v = new String('a', 10*1024*1024);" +
                                                    "    }" +
                                                    "}";
    }
}