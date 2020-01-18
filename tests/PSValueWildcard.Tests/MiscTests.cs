using System;
using Xunit;

using static PSValueWildcard.Tests.Util;

namespace PSValueWildcard.Tests
{
    public class MiscTests
    {
        [Fact]
        public void ShortExact()
        {
            Console.WriteLine("PID: {0}", System.Diagnostics.Process.GetCurrentProcess().Id);
            AssertMatch("test", "test");
            AssertNotMatch("tes", "test");
        }

        [Fact]
        public void MediumExact()
        {
            AssertMatch(
                "this is a pretty long string that is doing some things and should work.",
                "this is a pretty long string that is doing some things and should work.");
        }

        [Fact]
        public void NoImplicitStartOrEndWildcard()
        {
            AssertNotMatch("ttest", "test");
            AssertNotMatch("testt", "test");
            AssertNotMatch("ttestt", "test");
        }
    }
}
