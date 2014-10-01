using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.CSharp;
using Xunit;

namespace SafeRunnerTest
{
    public class ProcessRun
    {
        [Fact]
        public void Code_should_can_compile()
        {
            // arrange
            var compiler = new CSharpCodeProvider();
            var options = new CompilerParameters{GenerateExecutable = true};

            // act
            var result = compiler.CompileAssemblyFromSource(options, Code);

            // assert
            result.Errors.Count.Should().Be(0);
        }

        [Fact]
        public void we_can_find_process_by_its_name()
        {
            // arrange
            var ps = Process.Start("calc");

            // act
            var thePs = Process.GetProcessesByName("calc");

            // assert
            thePs.Length.Should().BeGreaterOrEqualTo(1);

            // clean
            ps.Kill();
        }

        [Fact]
        public void Emtpy_call_should_return_fail()
        {
            // arrange
            var empty = new ApiJudgeInfo();
            var result = new ApiJudgeResult();

            // act
            var ok = NativeDll.Judge(ref empty, ref result);

            // assert
            ok.Should().BeFalse();
        }

        [Fact]
        public void Create_calc_should_return_success()
        {
            // arrange
            var calc = new JudgeInfo
            {
                Path = "calc.exe", 
                MemoryLimitMb = 1.0f, 
                TimeLimitMs = 1000, 
                Input = null
            };

            // act
            var result = NativeDll.Judge(calc);

            // assert
            result.Succeed.Should().BeTrue();
            result.ErrorCode.Should().Be(0);
            result.ErrorMessage.Should().BeNull();
            result.MemoryMb.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Regular_clear_call_should_be_ok()
        {
            // arrange
            var result = new ApiJudgeResult();

            // act

            // assert
            Assert.DoesNotThrow(() => NativeDll.FreeJudgeResult(ref result));
            Assert.DoesNotThrow(() => NativeDll.FreeJudgeResult(ref result));
            Assert.DoesNotThrow(() => NativeDll.FreeJudgeResult(ref result));
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
            "            Console.InputEncoding = Encoding.Unicode;" +
            "            var str = Console.In.ReadToEnd();        " +
            "            Console.Write(\"Hey {0}!\", str);        " +
            "        }                                            " +
            "    }                                                " +
            "}                                                    ";
    }
}
