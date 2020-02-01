using Xunit;

using static PSValueWildcard.Tests.Util;

namespace PSValueWildcard.Tests
{
    public class AnyOneTests
    {
        [Fact]
        public void Basic()
        {
            AssertMatch("test", "te?t");
        }

        [Fact]
        public void AtEnd()
        {
            AssertMatch("test", "tes?");
            AssertNotMatch("test", "test?");
        }

        [Fact]
        public void AtStart()
        {
            AssertMatch("test", "?est");
            AssertNotMatch("test", "?test");
        }

        [Fact]
        public void AfterWildcard()
        {
            AssertMatch("testing", "test*?g");
        }

        [Fact]
        public void MakesWildcardRequireAtLeastOneChar()
        {
            AssertMatch("testing", "testi*?g");
            AssertNotMatch("testing", "testin*?g");
        }
    }
}
