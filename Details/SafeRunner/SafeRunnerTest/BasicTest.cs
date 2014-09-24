using FluentAssertions;
using Xunit;

namespace SafeRunnerTest
{
    public class BasicTest
    {
        [Fact]
        public void DllImport_should_not_throw_exception()
        {
            // arrange

            // act

            // assert
            Assert.DoesNotThrow(NativeDll.Cluck);
        }

        [Fact]
        public void Get3_should_really_returns_3()
        {
            // arrange

            // act
            int x = NativeDll.Get3();

            // assert
            x.Should().Be(3);
        }
    }
}
