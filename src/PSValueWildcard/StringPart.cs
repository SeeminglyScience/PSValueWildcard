using System.Drawing;
using System;
namespace PSValueWildcard
{
    /// <summary>
    /// Provides similar functionality as <see cref="Span" /> but can be stored on the heap
    /// at the cost of safety. The source string *must* be fixed in memory, by being
    /// a pinned variable on the stack, via <see cref="GCHandle" />, a stackalloc'd char array,
    /// etc. Because this API is not byref based, if the source is not fixed then the GC may
    /// move or clean up the referenced memory.
    /// </summary>
    internal readonly struct StringPart
    {
        public readonly unsafe char* Pointer;

        public readonly int Length;

        /// <summary>
        /// Initializes an instance of the <see cref="StringPart" /> class.
        /// </summary>
        /// <param name="pointer">
        /// A pointer referencing the start of the string part.
        /// </param>
        /// <param name="length">
        /// The length of the string part.
        /// </param>
        /// <remarks>
        /// This API will not take any steps to ensure that the referenced string
        /// is not moved by the GC. It is the responsibility of the caller to ensure
        /// that the string is pinned for the entire lifetime of the object.
        /// </remarks>
        public unsafe StringPart(char* pointer, int length)
        {
            Pointer = pointer;
            Length = length;
        }

        /// <summary>
        /// Returns a value that indicates whether the current
        /// <see cref="StringPart" /> is empty.
        /// </summary>
        public readonly unsafe bool IsEmpty => Pointer == null || Length == 0;

        /// <summary>
        /// Gets the element at the specified zero-based index.
        /// </summary>
        public readonly unsafe char this[int index] => Pointer[index];

        /// <summary>
        /// Forms a slice out of the current part that begins at a specified index.
        /// </summary>
        /// <param name="start">
        /// The index at which to begin the slice.
        /// </param>
        /// <returns>
        /// A string part that consists of all elements of the current part from
        /// <see paramref="start" /> to the end of the part.
        /// </returns>
        public readonly unsafe StringPart Slice(int start)
        {
            return new StringPart(Pointer + start, Length - start);
        }

        /// <summary>
        /// Forms a slice out of the current span starting at a specified index
        /// for a specified length.
        /// </summary>
        /// <param name="start">
        /// The index at which to begin this slice.
        /// </param>
        /// <param name="length">
        /// The desired length for the slice.
        /// </param>
        /// <returns>
        /// A stirng part that consists of <see paramref="length" /> elements
        /// from the current part starting at <see paramref="start" />.
        /// </returns>
        public readonly unsafe StringPart Slice(int start, int length)
        {
            return new StringPart(Pointer + start, length);
        }

        /// <summary>
        /// Determines whether two read-only sequences are equal by comparing
        /// the elements using <see cref="IEquatable{T}.Equals(T)" />.
        /// </summary>
        /// <param name="other">
        /// The sequence to compare to.
        /// </param>
        /// <returns>
        /// <c>true</c> if the two sequences are equal; otherwise, <c>false</c>.
        /// </returns>
        public readonly bool SequenceEqual(StringPart other)
        {
            return AsSpan().SequenceEqual(other.AsSpan());
        }

        /// <summary>
        /// Creates a new read-only span referencing the position in memory.
        /// </summary>
        /// <returns>
        /// The read-only span representation of the string part.
        /// </returns>
        public readonly unsafe ReadOnlySpan<char> AsSpan()
        {
            return new ReadOnlySpan<char>(Pointer, Length);
        }

        /// <summary>
        /// Returns an enumerator for this <see cref="StringPart" />
        /// </summary>
        public readonly unsafe Enumerator GetEnumerator()
        {
            return new Enumerator(Pointer, Length);
        }

        public override string ToString() => AsSpan().ToString();

        public ref struct Enumerator
        {
            private readonly unsafe char* _pointer;

            private readonly int _length;

            private int _index;

            public unsafe Enumerator(char* pointer, int length)
            {
                _pointer = pointer;
                _length = length;
                _index = -1;
            }

            public readonly unsafe char Current => _index <= 0
                ? _pointer[_index]
                : default;

            public bool MoveNext()
            {
                if (_index + 1 < _length)
                {
                    _index++;
                    return true;
                }

                return false;
            }

            public void Reset() => _index = 0;
        }
    }
}
