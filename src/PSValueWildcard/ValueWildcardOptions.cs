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

        /// <summary>
        /// Determines whether two specified <see cref="ValueWildcardOptions" /> instances
        /// have the same value.
        /// </summary>
        /// <param name="left">The first instance to compare.</param>
        /// <param name="right">The second instance to compare.</param>
        /// <returns>
        /// <c>true</c> if the value of <paramref name="left" /> is the same as the value
        /// of <paramref name="right" />; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(ValueWildcardOptions left, ValueWildcardOptions right)
            => left.Equals(right);

        /// <summary>
        /// Determines whether two specified <see cref="ValueWildcardOptions" /> instances
        /// have the different values.
        /// </summary>
        /// <param name="left">The first instance to compare.</param>
        /// <param name="right">The second instance to compare.</param>
        /// <returns>
        /// <c>true</c> if the value of <paramref name="left" /> is the different from the value
        /// of <paramref name="right" />; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(ValueWildcardOptions left, ValueWildcardOptions right)
            => !left.Equals(right);

        /// <summary>
        /// Returns a value that indicates whether this instance is equal to the specified
        /// <see cref="ValueWildcardOptions" /> instance.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="other" /> parameter equals the value of
        /// this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ValueWildcardOptions other)
        {
            return IsCaseSensitive == other.IsCaseSensitive
                && Culture == other.Culture;
        }

        /// <summary>
        /// Returns a value that indicates whether this instance is equal to the specified
        /// object or <see cref="ValueWildcardOptions" /> instance.
        /// </summary>
        /// <param name="obj">An object to compare to this instance.</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="obj" /> parameter equals the value of
        /// this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is ValueWildcardOptions other && Equals(other);

        /// <summary>Returns the hashcode for this string part.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => HashCodes.Combine(IsCaseSensitive, Culture);
    }
}
