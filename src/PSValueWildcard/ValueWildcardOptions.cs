using System;
using System.Diagnostics;
using System.Globalization;

namespace PSValueWildcard
{
    /// <summary>
    /// Provides options that alter the behavior of wildcard matching.
    /// </summary>
    [DebuggerDisplay("CS: {" + nameof(IsCaseSensitive) + "}, C: {" + nameof(_culture) + "}")]
    public readonly struct ValueWildcardOptions : IEquatable<ValueWildcardOptions>
    {
        private readonly CultureInfo? _culture;

        private ValueWildcardOptions(bool isCaseSensitive, CultureInfo? culture)
        {
            IsCaseSensitive = isCaseSensitive;
            _culture = culture;
        }

        /// <summary>
        /// Gets the default set of options for pattern matching behavior.
        /// </summary>
        public static ValueWildcardOptions Default => default;

        /// <summary>
        /// Gets wildcard options that are case sensitive and culture unaware.
        /// </summary>
        public static ValueWildcardOptions Ordinal => new ValueWildcardOptions(isCaseSensitive: true, culture: null);

        /// <summary>
        /// Gets wildcard options that are not case sensitive and culture unaware.
        /// </summary>
        public static ValueWildcardOptions InvariantIgnoreCase
            => new ValueWildcardOptions(isCaseSensitive: false, culture: null);

        /// <summary>
        /// Gets wildcard options that are not case sensitive and culture aware.
        /// </summary>
        public static ValueWildcardOptions CurrentCultureIgnoreCase
            => new ValueWildcardOptions(isCaseSensitive: false, culture: CultureInfo.CurrentCulture);

        /// <summary>
        /// Gets a value that indicates whether the pattern should consider case
        /// when evaluating a match.
        /// </summary>
        public readonly bool IsCaseSensitive { get; }

        /// <summary>
        /// Gets the culture that should be used when transforming a characters case.
        /// </summary>
        public readonly CultureInfo Culture => _culture ?? CultureInfo.InvariantCulture;

        public static bool operator ==(ValueWildcardOptions left, ValueWildcardOptions right)
            => left.Equals(right);

        public static bool operator !=(ValueWildcardOptions left, ValueWildcardOptions right)
            => !left.Equals(right);

        public bool Equals(ValueWildcardOptions other)
        {
            return IsCaseSensitive == other.IsCaseSensitive
                && Culture == other.Culture;
        }

        public override bool Equals(object? obj) => obj is ValueWildcardOptions other && Equals(other);

        public override int GetHashCode() => HashCodes.Combine(IsCaseSensitive, Culture);
    }
}
