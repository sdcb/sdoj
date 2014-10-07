using System;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using FluentAssertions;
using Microsoft.CSharp;
using Xunit;

namespace SafeRunnerTest
{
    public class SecurityTest
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
            Process ps = null;

            try
            {
                // Arrange
                ps = Process.Start("calc");
                Assert.NotNull(ps);

                var compiler = new CSharpCodeProvider();
                var options = new CompilerParameters
                {
                    GenerateExecutable = true
                };
                options.ReferencedAssemblies.Add("System.dll");
                var asm = compiler.CompileAssemblyFromSource(options, TerminateProcessByIdSource);
                asm.Errors.HasErrors.Should().BeFalse();

                var info = new JudgeInfo
                {
                    Input = ps.Id.ToString(CultureInfo.InvariantCulture),
                    MemoryLimitMb = 10.0f,
                    Path = asm.PathToAssembly,
                    TimeLimitMs = 1000
                };

                // Act
                var result = NativeDll.Judge(info);

                // Assert
                result.ErrorCode.Should().NotBe(0);
            }
            finally
            {
                // Clean up
                ps.Kill();
            }
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