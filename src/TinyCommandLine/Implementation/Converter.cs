using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.TinyCommandLine.Implementation
{
    static class Converter<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(ReadOnlySpan<char> str, out T value, out string error)
        {
            if (typeof(T) == typeof(string))
                return GetResult(str.ToString(), out value, out error);

            if (typeof(T) == typeof(char))
            {
                if (str.Length == 1)
                    return GetResult(str[0], out value, out error);

                return GetError("char", out value, out error);
            }

            if (typeof(T) == typeof(bool))
            {
                if (str.SequenceEqual("True") || str.SequenceEqual("true") || str.SequenceEqual("1"))
                    return GetResult(true, out value, out error);
                if (str.SequenceEqual("False") || str.SequenceEqual("false") || str.SequenceEqual("0"))
                    return GetResult(false, out value, out error);

                return GetError("bool", out value, out error);
            }

            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(str, out var result))
                    return GetResult(result, out value, out error);

                return GetError("int", out value, out error);
            }

            if (typeof(T) == typeof(double))
            {
                if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                    return GetResult(result, out value, out error);

                return GetError("double", out value, out error);
            }

            if (typeof(T) == typeof(DateTime))
            {
                if(DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var result))
                    return GetResult(result, out value, out error);

                return GetError(nameof(DateTime), out value, out error);
            }

            ThrowNotSupportedType();
            error = null;
            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool GetResult<TIn>(TIn value, out T result, out string error)
        {
            error = default;
            result = (T) (object) value;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool GetError(string type, out T result, out string error)
        {
            error = $"Option {{0}} must be a {type}.";
            result = default;
            return false;
        }

        static void ThrowNotSupportedType() => throw new NotSupportedException($"The '{typeof(T).Name}' is not supported");
    }
}