using System;
using System.CodeDom.Compiler;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CSharp;
using SdojJudger.Runner;
using SdojJudger.SandboxDll;
using Xunit;
using Xunit.Extensions;

namespace SandboxTests.RunTest.JudgeTest.SecurityTest
{
    [Trait("Run from Judge", "Memory Leak!")]
    public class MemoryLeakTest
    {
        [Theory]
        [InlineData(100)]
        public void Judge_run_many_times_should_not_leak_memory(int times)
        {
            // Arrange
            var compiler = new CSharpCodeProvider();
            var options = new CompilerParameters
            {
                GenerateExecutable = true
            };
            var asm = compiler.CompileAssemblyFromSource(options, Code);
            asm.Errors.HasErrors.Should().BeFalse();
            var info = new JudgeInfo
            {
                Input = "Flash", 
                MemoryLimitMb = 10.0f, 
                Path = asm.PathToAssembly, 
                TimeLimitMs = 150
            };
            var parallelOption = new ParallelOptions {MaxDegreeOfParallelism = 4};

            // Act & Assert
            Console.WriteLine(GC.GetTotalMemory(true));
            //for (int i = 0; i < times; ++i) Sandbox.Judge(info);
            Parallel.For(0, times, parallelOption, (i) => Sandbox.Judge(info));

            Console.WriteLine(GC.GetTotalMemory(true));
            Parallel.For(0, times, parallelOption, (i) => Sandbox.Judge(info));

            Console.WriteLine(GC.GetTotalMemory(true));
            Parallel.For(0, times, parallelOption, (i) => Sandbox.Judge(info));

            Console.WriteLine(GC.GetTotalMemory(true));
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