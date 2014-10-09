using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CSharp;
using Xunit;
using Xunit.Extensions;

namespace SafeRunnerTest.SecurityTest
{
    public class MemoryLeakTest
    {
        [Fact(Skip="性能测试")]
        [InlineData(1000)]
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

            // Act & Assert
            GC.Collect();
            Console.WriteLine(Process.GetCurrentProcess().VirtualMemorySize64);

            Parallel.For(0, times, (i) => NativeDll.Judge(info));

            GC.Collect();
            Console.WriteLine(Process.GetCurrentProcess().VirtualMemorySize64);
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