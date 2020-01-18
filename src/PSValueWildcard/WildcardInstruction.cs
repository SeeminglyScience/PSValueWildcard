using System;

namespace PSValueWildcard
{
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

        internal static WildcardInstruction AnyOf(StringPart arguments)
            => new WildcardInstruction(WildcardStepKind.AnyOf, arguments);

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

        public override string ToString()
        {
            return Kind switch
            {
                WildcardStepKind.AnyAny => "*",
                WildcardStepKind.AnyOne => "?",
                WildcardStepKind.AnyOf => string.Format("[{0}]", Args.ToString()),
                WildcardStepKind.Exact => Args.ToString(),
                _ => string.Empty,
            };
        }
    }
}
