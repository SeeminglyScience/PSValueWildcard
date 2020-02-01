using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PSValueWildcard
{
    [DebuggerDisplay("{GetDebuggerDisplayString()}")]
    internal readonly struct WildcardInstruction : IEquatable<WildcardInstruction>
    {
        public static readonly WildcardInstruction AnyAny = new WildcardInstruction(WildcardStepKind.AnyAny, default);

        public static readonly WildcardInstruction AnyOne = new WildcardInstruction(WildcardStepKind.AnyOne, default);

        public readonly WildcardStepKind Kind;

        internal readonly StringPart Args;

        private WildcardInstruction(WildcardStepKind kind, StringPart arguments)
        {
            Kind = kind;
            Args = arguments;
        }

        public readonly ReadOnlySpan<char> Arguments => Args.AsSpan();

        internal static WildcardInstruction AnyOf(StringPart arguments, bool isPartial = false)
            => new WildcardInstruction(
                isPartial ? WildcardStepKind.PartialAnyOf : WildcardStepKind.AnyOf,
                arguments);

        internal static WildcardInstruction PartialAnyOf(StringPart arguments)
            => new WildcardInstruction(WildcardStepKind.PartialAnyOf, arguments);

        internal static WildcardInstruction Exact(StringPart arguments)
            => new WildcardInstruction(WildcardStepKind.Exact, arguments);

        public static bool operator ==(WildcardInstruction left, WildcardInstruction right) => left.Equals(right);

        public static bool operator !=(WildcardInstruction left, WildcardInstruction right) => !left.Equals(right);

        public readonly bool TryGetLength(out int length)
        {
            length = Kind switch
            {
                WildcardStepKind.AnyOf => 1,
                WildcardStepKind.AnyOne => 1,
                WildcardStepKind.Exact => Args.Length,
                _ => -1,
            };

            return length != -1;
        }

        public readonly bool Equals(WildcardInstruction other)
        {
            return Kind == other.Kind && Args.SequenceEqual(other.Args);
        }

        public override bool Equals(object? obj) => obj is WildcardInstruction other && Equals(other);

        public override int GetHashCode() => HashCodes.Combine(Kind, Args);

#if DEBUG
        [ExcludeFromCodeCoverage]
        private string GetDebuggerDisplayString()
        {
            return Kind switch
            {
                WildcardStepKind.AnyAny => "*",
                WildcardStepKind.AnyOne => "?",
                WildcardStepKind.AnyOf => string.Format("AO: {0}", Args.ToString()),
                WildcardStepKind.PartialAnyOf => string.Format("PAO: {0}", Args.ToString()),
                WildcardStepKind.Exact => string.Format("E: {0}", Args.ToString()),
                _ => string.Empty,
            };
        }
#endif
    }
}
