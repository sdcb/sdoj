using System;
using System.Diagnostics;
using System.IO;
using FluentAssertions;
using SdojJudger.SandboxDll;
using Xunit;

namespace SandboxTests.RunTest
{
    public class BasicRunTest
    {
        [Fact]
        [Trait("Run", "Basic")]
        public void Regular_run_should_be_ok()
        {
            // Arrange
            var info = new SandboxRunInfo
            {
                LimitProcessCount = false,
                MemoryLimitMb = 10.0f,
                Path = "whoami",
                TimeLimitMs = 1000
            };

            // Act
            var ior = Sandbox.BeginRun(info);

            // Assert
            ior.Succeed.Should().BeTrue();
            ior.ErrorCode.Should().Be(0);

            ior.ErrorReadStream.Should().NotBeNull();
            ior.ErrorReadStream.CanRead.Should().BeTrue();

            ior.OutputReadStream.Should().NotBeNull();
            ior.OutputReadStream.CanRead.Should().BeTrue();

            ior.InputWriteStream.Should().NotBeNull();
            ior.InputWriteStream.CanWrite.Should().BeTrue();

            ior.InstanceHandle.Should().NotBeNull();

            var res = Sandbox.EndRun(ior.InstanceHandle);
            res.Succeed.Should().BeTrue();
            res.MemoryMb.Should().BeGreaterThan(0);
            res.TimeMs.Should().BeGreaterOrEqualTo(0);

            using (var reader = new StreamReader(ior.OutputReadStream))
            {
                var content = reader.ReadToEnd();
                var actual = content.Split(new[] {'\\'});
                var actualDomain = actual[0];
                var actualUsername = actual[1].Trim();

                var domain = Environment.UserDomainName;
                var username = Environment.UserName;

                Assert.Equal(domain, actualDomain.ToUpperInvariant());
                Assert.Equal(username, actualUsername);
            }
            Process.GetProcessesByName("whoami").Length.Should().Be(0);
        }
    }
}
