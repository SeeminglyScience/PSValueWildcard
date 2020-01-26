using System;
using System.Runtime.InteropServices;

namespace PSValueWildcard
{
    internal static class MemoryMarshalPoly
    {
        public static ref T GetReference<T>(Span<T> span)
        {
#if CORE
            return ref MemoryMarshal.GetReference(span);
#else
            if (span.IsEmpty)
            {
                unsafe
                {
                    return ref System.Runtime.CompilerServices.Unsafe.AsRef<T>(null);
                }
            }

            return ref span[0];
#endif
        }
    }
}
