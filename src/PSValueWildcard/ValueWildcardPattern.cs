using System;

namespace PSValueWildcard
{
    /// <summary>
    /// Provides span based wildcard pattern processing.
    /// </summary>
    public static class ValueWildcardPattern
    {
        /// <summary>
        /// Indicates whether the specified wildcard pattern finds a match
        /// in the specified input string.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The wildcard pattern to match.</param>
        /// <returns>
        /// <c>true</c> if the regular expression finds a match; otherwise,
        /// <c>false</c>.
        /// </returns>
        public static unsafe bool IsMatch(string input, string pattern)
        {
            return IsMatch(input, pattern, options: default);
        }

        /// <summary>
        /// Indicates whether the specified wildcard pattern finds a match
        /// in the specified input string.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The wildcard pattern to match.</param>
        /// <param name="options">Options that alter the behavior of the matcher.</param>
        /// <returns>
        /// <c>true</c> if the regular expression finds a match; otherwise,
        /// <c>false</c>.
        /// </returns>
        public static unsafe bool IsMatch(string input, string pattern, ValueWildcardOptions options)
        {
            fixed (char* pInput = input)
            fixed (char* pPattern = pattern)
            {
                return WildcardInterpreter.IsMatch(pInput, input.Length, pPattern, pattern.Length, options);
            }
        }

        /// <summary>
        /// Indicates whether the specified wildcard pattern finds a match
        /// in the specified input string.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The wildcard pattern to match.</param>
        /// <returns>
        /// <c>true</c> if the regular expression finds a match; otherwise,
        /// <c>false</c>.
        /// </returns>
        public static unsafe bool IsMatch(ReadOnlySpan<char> input, ReadOnlySpan<char> pattern)
        {
            return IsMatch(input, pattern, options: default);
        }

        /// <summary>
        /// Indicates whether the specified wildcard pattern finds a match
        /// in the specified input string.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="pattern">The wildcard pattern to match.</param>
        /// <param name="options">Options that alter the behavior of the matcher.</param>
        /// <returns>
        /// <c>true</c> if the regular expression finds a match; otherwise,
        /// <c>false</c>.
        /// </returns>
        public static unsafe bool IsMatch(
            ReadOnlySpan<char> input,
            ReadOnlySpan<char> pattern,
            ValueWildcardOptions options)
        {
            fixed (char* pInput = input)
            fixed (char* pPattern = pattern)
            {
                return WildcardInterpreter.IsMatch(pInput, input.Length, pPattern, pattern.Length, options);
            }
        }

        /// <summary>
        /// Creates an object that represents a preprocessed wildcard pattern.
        /// The source pattern string is pinned via a <see cref="GCHandle" /> to allow
        /// it to be safely stored on the heap. You must call <see cref="ParsedWildcardPattern.Dispose" />
        /// when the pattern is no longer required to free the pinned string.
        /// </summary>
        /// <param name="pattern">The wildcard pattern to process.</param>
        /// <returns>
        /// The preprocessed wildcard pattern.
        /// </returns>
        public static ParsedWildcardPattern Parse(string pattern)
        {
            return ParsedWildcardPattern.ParseAndAlloc(pattern);
        }
    }
}
