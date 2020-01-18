#if NETSTANDARD20

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Specifies that <c>null</c> is allowed as an input even if the
    /// corresponding type disallows it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    internal sealed class AllowNullAttribute : Attribute
    {
    }

    /// <summary>
    /// Specifies that <c>null</c> is disallowed as an input even if the
    /// corresponding type allows it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    internal sealed class DisallowNullAttribute : Attribute
    {
    }

    /// <summary>
    /// Specifies that an output may be <c>null</c> even if the corresponding
    /// type disallows it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
    internal sealed class MaybeNullAttribute : Attribute
    {
    }
}
#endif
