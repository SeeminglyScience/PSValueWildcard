namespace PSValueWildcard
{
    internal static class HashCodes
    {
        public static int Combine<T1, T2>(T1 value1, T2 value2)
        {
#if CORE
            return System.HashCode.Combine(value1, value2);
#else
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + value1?.GetHashCode() ?? 0;
                hash = (hash * 23) + value2?.GetHashCode() ?? 0;
                return hash;
            }
#endif
        }
    }
}
