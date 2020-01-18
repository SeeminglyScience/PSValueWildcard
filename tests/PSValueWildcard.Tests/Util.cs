using Xunit;

namespace PSValueWildcard.Tests
{
    public static class Util
    {
        public static void AssertMatch(string input, string pattern)
        {
            Assert.True(
                ValueWildcardPattern.IsMatch(
                    input,
                    pattern),
                string.Format(
                    "\"{0}\" -like \"{1}\"",
                    input,
                    pattern));
        }

        public static void AssertNotMatch(string input, string pattern)
        {
            Assert.False(
                ValueWildcardPattern.IsMatch(
                    input,
                    pattern),
                string.Format(
                    "\"{0}\" -notlike \"{1}\"",
                    input,
                    pattern));
        }
    }
}
