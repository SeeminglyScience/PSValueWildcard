using Xunit;

using static PSValueWildcard.Tests.Util;

namespace PSValueWildcard.Tests
{
    public class BacktrackingTests
    {
        [Fact]
        public void MatchAtBacktrack()
        {
            AssertMatch("something test wrong test right", "*test [r]*");
            AssertNotMatch("something test wrong test right", "*test [x]*");
        }

        [Fact]
        public void FindNextBacktrack()
        {
            AssertMatch("something test wrong test right", "something*[rt]ight");
        }
    }
}
