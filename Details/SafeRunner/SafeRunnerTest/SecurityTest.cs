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
        public void Process_should_not_terminate_other_process()
        {
            Process ps = null;

            try
            {
                // Arrange
                const string source = "using System;" +
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
                ps = Process.Start("calc");
                Assert.NotNull(ps);

                var compiler = new CSharpCodeProvider();
                var options = new CompilerParameters
                {
                    GenerateExecutable = true
                };
                options.ReferencedAssemblies.Add("System.dll");
                var asm = compiler.CompileAssemblyFromSource(options, source);
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
    }
}