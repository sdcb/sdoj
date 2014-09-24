using System.Text;
using FluentAssertions;
using Xunit;
using Xunit.Extensions;

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
        public void Get3_should_returns_3()
        {
            // arrange

            // act
            int x = NativeDll.Get3();

            // assert
            x.Should().Be(3);
        }

        [Theory]
        [InlineData("Hello World", 11)]
        [InlineData("", 0)]
        [InlineData(null, -1)]
        public void StringLength_should_count_length_of_string(string text, int expect)
        {
            // arrange

            // act
            int actValue = NativeDll.StringLength(text);

            // assert
            actValue.Should().Be(expect);
        }

        [Theory]
        [InlineData("A", "B", "C", 4, true)]
        [InlineData("", "", "", 1, true)]
        [InlineData("", "", "", 0, false)]
        public void ConcatStringTable_should_concat_string(string s1, string s2, string s3, int length, bool expected)
        {
            // arrange
            var table = new NativeDll.StringTable
            {
                String1 = s1,
                String2 = s2,
                String3 = s3,
                Length1 = s1.Length,
                Length2 = s2.Length,
                Length3 = s3.Length
            };
            var buffer = new StringBuilder(length);

            // act
            var ret = NativeDll.ConcatStringTable(ref table, buffer, length);

            // assert
            ret.Should().Be(expected);
            if (ret)
            {
                buffer.ToString().Should().Be(s1 + s2 + s3);
            }
        }

        [Theory]
        [InlineData("A", "B", "C", 4, true)]
        [InlineData("", "", "", 1, true)]
        [InlineData("", "", "", 0, false)]
        public void ConcatStringArgs_should_concat_string(string s1, string s2, string s3, int length, bool expected)
        {
            // arrange
            var buffer = new StringBuilder(length);

            // act
            var ret = NativeDll.ConcatStringArgs(s1, s1.Length, s2, s2.Length, s3, s3.Length, buffer, length);

            // assert
            ret.Should().Be(expected);
            if (ret)
            {
                buffer.ToString().Should().Be(s1 + s2 + s3);
            }
        }
    }
}