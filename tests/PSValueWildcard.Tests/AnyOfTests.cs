using Xunit;

using static PSValueWildcard.Tests.Util;

namespace PSValueWildcard.Tests
{
    public class AnyOfTests
    {
        [Fact]
        public void Basic()
        {
            AssertMatch("test", "te[st]t");
        }

        [Fact]
        public void Many()
        {
            AssertMatch("test", "te[stagfxcbv]t");
            AssertMatch("tett", "te[stagfxcbv]t");
            AssertMatch("teat", "te[stagfxcbv]t");
            AssertMatch("tegt", "te[stagfxcbv]t");
            AssertMatch("teft", "te[stagfxcbv]t");
            AssertMatch("text", "te[stagfxcbv]t");
            AssertMatch("tect", "te[stagfxcbv]t");
            AssertMatch("tebt", "te[stagfxcbv]t");
            AssertMatch("tevt", "te[stagfxcbv]t");
            AssertNotMatch("teqt", "te[stagfxcbv]t");
        }

        [Fact]
        public void SpecialCharactersCountAsLiterals()
        {
            AssertMatch("tes*t", "tes[*[?]t");
            AssertMatch("tes[t", "tes[*[?]t");
            AssertMatch("tes?t", "tes[*[?]t");
        }

        [Fact]
        public void CanEscapeCharacters()
        {
            AssertMatch("testt", "tes[`]t]t");
            AssertMatch("tes]t", "tes[`]t]t");
            AssertMatch("tes]t", "tes[`]`tfajsd`*qw]t");
            AssertNotMatch("tes`t", "tes[`]t]t");
        }
    }
}
