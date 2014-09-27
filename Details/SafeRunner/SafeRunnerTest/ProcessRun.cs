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
            var empty = new NativeDll.ApiJudgeInfo();
            var result = new NativeDll.ApiJudgeResult();

            // act
            var ok = NativeDll.Judge(ref empty, ref result);

            // assert
            ok.Should().BeFalse();
        }

        [Fact]
        public void Create_calc_should_return_success()
        {
            // arrange
            var empty = new NativeDll.ApiJudgeInfo();
            empty.Path = "calc.exe";
            empty.PathLength = empty.Path.Length;
            empty.MemoryLimitMb = 1;
            empty.TimeLimitMs = 1000;
            var result = new NativeDll.ApiJudgeResult();

            // act
            var ok = NativeDll.Judge(ref empty, ref result);
            var psCalc = Process.GetProcessesByName("calc.exe");

            // assert
            ok.Should().BeTrue();
        }

        [Fact]
        public void Regular_clear_call_should_be_ok()
        {
            // arrange
            var result = new NativeDll.ApiJudgeResult();

            // act

            // assert
            Assert.DoesNotThrow(() => NativeDll.FreeJudgeResult(ref result));
            Assert.DoesNotThrow(() => NativeDll.FreeJudgeResult(ref result));
            Assert.DoesNotThrow(() => NativeDll.FreeJudgeResult(ref result));
        }

        [Fact]
        public void Process_should_return_expected_output()
        {
            
        }

        public const string Code = 
            "using System;                                    " +
            "                                                 " +
            "namespace CsConsole                              " +
            "{                                                " +
            "    class Program                                " +
            "    {                                            " +
            "        static void Main(string[] args)          " +
            "        {                                        " +
            "            var str = Console.In.ReadToEnd();    " +
            "            Console.WriteLine(\"Hey {0}!\", str);" +
            "        }                                        " +
            "    }                                            " +
            "}                                                ";
    }
}
