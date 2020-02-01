using System;
using System.Globalization;

namespace PSValueWildcard
{
    internal static class Error
    {
        public static InvalidOperationException InvalidWildcardPattern(string pattern)
        {
            return new InvalidOperationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.InvalidWildcardPattern,
                    pattern));
        }

        public static ArgumentNullException ArgumentNull(string parameterName)
        {
            return new ArgumentNullException(parameterName);
        }

        public static ArgumentOutOfRangeException ArgumentOutOfRange(string parameterName)
        {
            return new ArgumentOutOfRangeException(parameterName);
        }

        public static ObjectDisposedException ObjectDisposed(string objectName)
        {
            return new ObjectDisposedException(objectName);
        }
    }
}
