using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.TinyCommandLine.Implementation
{
    static class Converter<T>
    {
        // This method will be optimized with jit and should do zero cpu instruction
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Cast<TIn>(TIn value) => (T)(object)value;

        public static T Parse(string str, string optionName)
        {
            if (typeof(T) == typeof(string))
                return Cast(str);

            if (typeof(T) == typeof(char))
            {
                return str.Length == 1
                    ? Cast(str[0])
                    : throw ExceptionHelper.InvalidOptionType(optionName, "char");
            }

            if (typeof(T) == typeof(bool))
            {
                if (str == "True" || str == "true" || str == "1")
                    return Cast(true);
                if (str == "False" || str == "false" || str == "0")
                    return Cast(false);

                throw ExceptionHelper.InvalidOptionType(optionName, "bool");
            }

            if (typeof(T) == typeof(int))
            {
                return int.TryParse(str, out var result)
                    ? Cast(result)
                    : throw ExceptionHelper.InvalidOptionType(optionName, "int");
            }

            if (typeof(T) == typeof(double))
            {
                return double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
                    ? Cast(result)
                    : throw ExceptionHelper.InvalidOptionType(optionName, "double");
            }

            if (typeof(T) == typeof(DateTime))
            {
                return DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var result)
                    ? Cast(result)
                    : throw ExceptionHelper.InvalidOptionType(optionName, nameof(DateTime));
            }

            throw new NotSupportedException($"The type of option '{optionName}' is not supported");
        }
    }
}