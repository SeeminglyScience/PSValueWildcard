using Xunit;

namespace PSValueWildcard.Tests
{
    public static class Util
    {
        public static void AssertMatch(string input, string pattern, bool caseSensitiveShouldFail = false)
        {
            TestOnDemand(input, pattern, shouldPass: true, caseSensitive: false);
            TestOnDemand(input, pattern, shouldPass: !caseSensitiveShouldFail, caseSensitive: true);
            TestHeapAllocated(input, pattern, shouldPass: true, caseSensitive: false);
            TestHeapAllocated(input, pattern, !caseSensitiveShouldFail, caseSensitive: true);
        }

        public static void AssertNotMatch(string input, string pattern, bool caseSensitiveShouldPass = false)
        {
            TestOnDemand(input, pattern, shouldPass: false, caseSensitive: false);
            TestOnDemand(input, pattern, shouldPass: caseSensitiveShouldPass, caseSensitive: true);
            TestHeapAllocated(input, pattern, shouldPass: false, caseSensitive: false);
            TestHeapAllocated(input, pattern, caseSensitiveShouldPass, caseSensitive: true);
        }

        private static void TestOnDemand(string input, string pattern, bool shouldPass, bool caseSensitive)
        {
            string because = string.Format(
                "\"{0}\" -{1}{2}like \"{3}\" (OnDemand)",
                input,
                caseSensitive ? 'c' : 'i',
                shouldPass ? "" : "not",
                pattern);

            ValueWildcardOptions options = caseSensitive
                ? ValueWildcardOptions.Ordinal
                : ValueWildcardOptions.InvariantIgnoreCase;

            if (shouldPass)
            {
                Assert.True(
                    ValueWildcardPattern.IsMatch(
                        input,
                        pattern,
                        options),
                    because);
                return;
            }

            Assert.False(
                ValueWildcardPattern.IsMatch(
                    input,
                    pattern,
                    options),
                because);
        }

        private static void TestHeapAllocated(string input, string pattern, bool shouldPass, bool caseSensitive)
        {
            string because = string.Format(
                "\"{0}\" -{1}{2}like \"{3}\" (Heap)",
                input,
                caseSensitive ? 'c' : 'i',
                shouldPass ? "" : "not",
                pattern);

            ValueWildcardOptions options = caseSensitive
                ? ValueWildcardOptions.Ordinal
                : ValueWildcardOptions.InvariantIgnoreCase;

            using var allocatedPattern = ValueWildcardPattern.Parse(pattern);
            if (shouldPass)
            {
                Assert.True(allocatedPattern.IsMatch(input, options), because);
                return;
            }

            Assert.False(allocatedPattern.IsMatch(input, options), because);
        }
    }
}
