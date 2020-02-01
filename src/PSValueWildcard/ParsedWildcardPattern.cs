using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PSValueWildcard
{
    /// <summary>
    /// Represents a parsed wildcard pattern that is ready to match.
    /// </summary>
    public sealed class ParsedWildcardPattern : IDisposable
    {
        private static readonly ParsedWildcardPattern Empty = new ParsedWildcardPattern(default, default);

        private bool _isDisposed;

        private GCHandle _handle;

        private ReadOnlyMemory<WildcardInstruction> _instructions;

        private ParsedWildcardPattern(GCHandle handle, ReadOnlyMemory<WildcardInstruction> instructions)
        {
            _handle = handle;
            _instructions = instructions;
        }

        internal static unsafe ParsedWildcardPattern ParseAndAlloc(string pattern)
        {
            if (pattern == null)
            {
                throw Error.ArgumentNull(nameof(pattern));
            }

            if (pattern.Length == 0)
            {
                return Empty;
            }

            ParsedWildcardPattern result;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                var handle = GCHandle.Alloc(pattern, GCHandleType.Pinned);
                try
                {
                    using var parser = new WildcardParser(
                        initialBuffer: default,
                        new StringPart(
                            (char*)handle.AddrOfPinnedObject().ToPointer(),
                            pattern.Length));

                    parser.Parse();
                    result = new ParsedWildcardPattern(
                        handle,
                        parser.Steps.ToArray().AsMemory());
                }
                catch
                {
                    handle.Free();
                    throw;
                }
            }

            return result;
        }

        /// <summary>
        /// Indicates whether the wildcard pattern specified in the creation of
        /// this object finds a match in the input string.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <returns>
        /// <c>true</c> if the wildcard pattern finds a match; otherwise, <c>false</c>.
        /// </returns>
        public unsafe bool IsMatch(string input)
        {
            return IsMatch(input.AsSpan(), options: default);
        }

        /// <summary>
        /// Indicates whether the wildcard pattern specified in the creation of
        /// this object finds a match in the input string.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="options">Options that alter the behavior of the matcher.</param>
        /// <returns>
        /// <c>true</c> if the wildcard pattern finds a match; otherwise, <c>false</c>.
        /// </returns>
        public unsafe bool IsMatch(string input, ValueWildcardOptions options)
        {
            return IsMatch(input.AsSpan(), options);
        }

        /// <summary>
        /// Indicates whether the wildcard pattern specified in the creation of
        /// this object finds a match in the input string.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <returns>
        /// <c>true</c> if the wildcard pattern finds a match; otherwise, <c>false</c>.
        /// </returns>
        public unsafe bool IsMatch(ReadOnlySpan<char> input)
        {
            return IsMatch(input, options: default);
        }

        /// <summary>
        /// Indicates whether the wildcard pattern specified in the creation of
        /// this object finds a match in the input string.
        /// </summary>
        /// <param name="input">The string to search for a match.</param>
        /// <param name="options">Options that alter the behavior of the matcher.</param>
        /// <returns>
        /// <c>true</c> if the wildcard pattern finds a match; otherwise, <c>false</c>.
        /// </returns>
        public unsafe bool IsMatch(ReadOnlySpan<char> input, ValueWildcardOptions options)
        {
            if (_isDisposed)
            {
                throw Error.ObjectDisposed(nameof(ParsedWildcardPattern));
            }

            char* pInput = (char*)Unsafe.AsPointer(ref Unsafe.AsRef(MemoryMarshalPoly.GetReference(input)));
            return WildcardInterpreter.IsMatch(
                new StringPart(pInput, input.Length),
                _instructions.Span,
                options);
        }

        /// <summary>
        /// Frees the pinned string referenced by this pattern. This will
        /// suppress the finalizer on the object from getting called by
        /// calling <see cref="GC.SuppressFinalize(object)" />.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _handle.Free();
            _handle = default;
            _instructions = default;

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ParsedWildcardPattern" /> is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        /// This method overrides <see cref="object.Finalize" />. Application code should not
        /// call this method; an object's Finalize method is automatically invoked during
        /// garbage collection, unless finalization by the garbage collector has been disabled
        /// by a call to the <see cref="GC.SuppressFinalize(object)" /> method.
        /// </remarks>
        ~ParsedWildcardPattern() => Dispose();
    }
}
