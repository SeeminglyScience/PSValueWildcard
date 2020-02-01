using Xunit;

using static PSValueWildcard.Tests.Util;

namespace PSValueWildcard.Tests
{
    public class HeapAllocationTests
    {
        [Fact]
        public void MoreInstructionsThanEstimated()
        {
            // A stackalloc buffer is pass to the parser based on an estimate
            // of characters per instruction. If there are more instructions
            // than the average, StackLoadedValueList will allocate.
            Assert.True(
                ValueWildcardPattern.IsMatch(
                    "testing",
                    "???????"));
        }

        [Fact]
        public void LongAnyOfPart()
        {
            // When combining partial instructions, a very large AnyOf that
            // must be broken up due to escapes can cause allocation.
            var pattern = "[a`" + new string('c', 0x200) + "]";
            Assert.True(ValueWildcardPattern.IsMatch("c", pattern));
        }
    }
}
