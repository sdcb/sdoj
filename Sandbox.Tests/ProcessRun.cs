using System.Diagnostics;
using FluentAssertions;
using SdojJudger.Runner;
using Xunit;

namespace Sandbox.Tests
{
    public class ProcessRun
    {
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
        public void Create_calc_should_return_success()
        {
            // arrange
            var calc = new JudgeInfo
            {
                Path = "calc.exe", 
                MemoryLimitMb = 10.0f, 
                TimeLimitMs = 100, 
                Input = null
            };

            // act
            var result = NativeDll.Judge(calc);

            // assert
            result.Succeed.Should().BeTrue();
            result.ErrorCode.Should().Be(0);
            result.MemoryMb.Should().BeGreaterThan(0);
        }
    }
}
