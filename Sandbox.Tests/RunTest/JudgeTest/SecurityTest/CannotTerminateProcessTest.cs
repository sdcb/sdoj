using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Globalization;
using FluentAssertions;
using Microsoft.CSharp;
using SdojJudger.Runner;
using SdojJudger.SandboxDll;
using Xunit;

namespace SandboxTests.RunTest.JudgeTest.SecurityTest
{
    [Trait("Run from Judge", "Security")]
    public class CannotTerminateProcessTest
    {
        [Fact]
        public void TerminateProcessById_should_can_compile()
        {
            // Arrange
            var compiler = new CSharpCodeProvider();
            var options = new CompilerParameters
            {
                GenerateExecutable = true
            };
            options.ReferencedAssemblies.Add("System.dll");

            // Arrange
            var asm = compiler.CompileAssemblyFromSource(options, TerminateProcessByIdSource);

            // Assert
            asm.Errors.HasErrors.Should().BeFalse();
        }

        [Fact]
        public void Process_should_not_terminate_other_process()
        {
            // Arrange
            var compiler = new CSharpCodeProvider();
            var options = new CompilerParameters
            {
                GenerateExecutable = true
            };
            options.ReferencedAssemblies.Add("System.dll");
            var asm = compiler.CompileAssemblyFromSource(options, TerminateProcessByIdSource);
            asm.Errors.HasErrors.Should().BeFalse();

            var ps = Process.Start("calc");
            var info = new JudgeInfo
            {
                Input = ps.Id.ToString(CultureInfo.InvariantCulture),
                MemoryLimitMb = 10.0f,
                Path = asm.PathToAssembly,
                TimeLimitMs = 100
            };

            // Act
            var result = Sandbox.Judge(info);
            var that = Process.GetProcessById(ps.Id);

            // Assert
            result.ExitCode.Should().NotBe(0);
            that.Should().NotBeNull();

            // Clean up
            ps.Kill();
        }

        [Fact]
        public void Not_using_judge_should_exit_calc()
        {
            // Arrange
            var compiler = new CSharpCodeProvider();
            var options = new CompilerParameters
            {
                GenerateExecutable = true
            };
            options.ReferencedAssemblies.Add("System.dll");
            var asm = compiler.CompileAssemblyFromSource(options, TerminateProcessByIdSource);

            var pi = new ProcessStartInfo(asm.PathToAssembly)
            {
                RedirectStandardInput = true, 
                UseShellExecute = false, 
            };

            // Act
            var ps = Process.Start("calc");
            var asmPs = Process.Start(pi);
            asmPs.StandardInput.Write(ps.Id);
            asmPs.StandardInput.Close();
            asmPs.WaitForExit();

            // Assert
            Assert.Throws<ArgumentException>(() =>
            {
                Process.GetProcessById(ps.Id);
            });
        }

        private const string TerminateProcessByIdSource = "using System;" +
                                                          "using System.Diagnostics;" +
                                                          "" +
                                                          "class Program" +
                                                          "{" +
                                                          "    static void Main(string[] args)" +
                                                          "    {" +
                                                          "        var line = Console.ReadLine();" +
                                                          "        var id = int.Parse(line);" +
                                                          "        var ps = Process.GetProcessById(id);" +
                                                          "        ps.Kill();" +
                                                          "    }" +
                                                          "}";
    }
}