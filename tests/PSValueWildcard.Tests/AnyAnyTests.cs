using Xunit;

using static PSValueWildcard.Tests.Util;

namespace PSValueWildcard.Tests
{
    public class AnyAnyTests
    {
        [Fact]
        public void StartWildcard()
        {
            AssertMatch("there are some thing before test", "*test");
        }

        [Fact]
        public void EndWildcard()
        {
            AssertMatch("test there are some things after", "test*");
        }

        [Fact]
        public void MiddleWildcard()
        {
            AssertMatch("test there are some things after", "test*after");
            AssertNotMatch("test there are some things after", "test*incorrect");
            AssertNotMatch("test there are some things after", "test*[x]");
        }

        [Fact]
        public void CanBeZeroCharacters()
        {
            AssertMatch("test", "test*");
            AssertMatch("test", "*test");
            AssertMatch("test", "te*st");
            AssertMatch("", "*");
        }

        [Fact]
        public void Multiple()
        {
            AssertMatch("this test should work", "this*should*");
            AssertMatch("this test should work", "this*sh*work");
            AssertMatch("this test should work", "*his*sh*work");
            AssertMatch("this test should work", "*his*sh*wor*");
        }
    }
}
