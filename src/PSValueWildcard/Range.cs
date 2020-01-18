using System;
using System.ComponentModel;

namespace PSValueWildcard
{
    internal readonly struct Range : IEquatable<Range>
    {
        private static readonly Range s_notFound = new Range(-1, 0);

        public readonly int Start;

        public readonly int End;

        public Range(int start, int end)
        {
            Start = start;
            End = end;
        }

        public static ref readonly Range NotFound => ref s_notFound;

        public readonly bool IsInvalid => Start < 0 || End < Start;

        public readonly bool IsNotFound => Start == -1 && End == 0;

        public static implicit operator Range((int start, int end) source)
        {
            return new Range(source.start, source.end);
        }

        public static implicit operator (int start, int end)(Range source)
        {
            return (source.Start, source.End);
        }

        public static bool operator ==(Range left, Range right) => left.Equals(right);

        public static bool operator !=(Range left, Range right) => !left.Equals(right);

        public readonly Range With(int? start = null, int? end = null)
        {
            return new Range(start ?? Start, end ?? End);
        }

        public readonly override bool Equals(object? obj) => obj is Range other && Equals(other);

        public readonly override int GetHashCode() => HashCodes.Combine(Start, End);

        public readonly override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{{ {0}, {1} }}",
                Start,
                End);
        }

        public readonly bool Equals(Range other)
        {
            return Start == other.Start && End == other.End;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public readonly void Deconstruct(out int start, out int end)
        {
            start = Start;
            end = End;
        }
    }
}
