using System.CodeDom.Compiler;
using FluentAssertions;
using Microsoft.CSharp;
using SdojJudger.Runner;
using Xunit;

namespace SandboxTests.JudgeTest
{
    public class BasicJudgeTest
    {
        [Fact]
        public void Code_should_can_compile()
        {
            // arrange
            var compiler = new CSharpCodeProvider();
            var options = new CompilerParameters { GenerateExecutable = true };

            // act
            var result = compiler.CompileAssemblyFromSource(options, Code);

            // assert
            result.Errors.Count.Should().Be(0);
        }

        [Fact]
        public void Process_should_return_expected_output()
        {
            // Arrange
            var compiler = new CSharpCodeProvider();
            var options = new CompilerParameters { GenerateExecutable = true };
            var res = compiler.CompileAssemblyFromSource(options, Code);
            var info = new JudgeInfo
            {
                Input = "Flash",
                MemoryLimitMb = 10,
                Path = res.PathToAssembly,
                TimeLimitMs = 1000
            };

            // Act
            var result = NativeDll.Judge(info);

            // Assert
            result.Succeed.Should().BeTrue();
            result.Output.Should().Be("Hey Flash!");
        }

        [Fact]
        public void Ten_kb_input_should_work_well()
        {
            // Arrange
            var compiler = new CSharpCodeProvider();
            var options = new CompilerParameters { GenerateExecutable = true };
            var res = compiler.CompileAssemblyFromSource(options, Code);
            var info = new JudgeInfo
            {
                Input = new string('F', 10 * 1024),
                MemoryLimitMb = 10,
                Path = res.PathToAssembly,
                TimeLimitMs = 1000
            };
            var expectedOutput = string.Format("Hey {0}!", info.Input);

            // Act
            var result = NativeDll.Judge(info);

            // Assert
            result.Succeed.Should().BeTrue();
            result.Output.Should().Be(expectedOutput);
        }

        public const string Code =
            "using System;                                        " +
            "using System.Text;                                   " +
            "                                                     " +
            "namespace CsConsole                                  " +
            "{                                                    " +
            "    class Program                                    " +
            "    {                                                " +
            "        static void Main(string[] args)              " +
            "        {                                            " +
            "            var str = Console.In.ReadToEnd();        " +
            "            Console.Write(\"Hey {0}!\", str);        " +
            "        }                                            " +
            "    }                                                " +
            "}                                                    ";
    }
}
