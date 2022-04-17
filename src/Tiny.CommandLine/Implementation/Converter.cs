using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Tiny.CommandLine.Implementation
{
    interface IParser<TVal> { bool TryParse(ReadOnlySpan<char> str, out TVal value); }

    struct ParserI64 : IParser<long> { public bool TryParse(ReadOnlySpan<char> str, out long value) => long.TryParse(str, out value); }
    struct ParserU64 : IParser<ulong> { public bool TryParse(ReadOnlySpan<char> str, out ulong value) => ulong.TryParse(str, out value); }
    struct ParserI32 : IParser<int> { public bool TryParse(ReadOnlySpan<char> str, out int value) => int.TryParse(str, out value); }
    struct ParserU32 : IParser<uint> { public bool TryParse(ReadOnlySpan<char> str, out uint value) => uint.TryParse(str, out value); }
    struct ParserI16 : IParser<short> { public bool TryParse(ReadOnlySpan<char> str, out short value) => short.TryParse(str, out value); }
    struct ParserU16 : IParser<ushort> { public bool TryParse(ReadOnlySpan<char> str, out ushort value) => ushort.TryParse(str, out value); }
    struct ParserI8 : IParser<sbyte> { public bool TryParse(ReadOnlySpan<char> str, out sbyte value) => sbyte.TryParse(str, out value); }
    struct ParserU8 : IParser<byte> { public bool TryParse(ReadOnlySpan<char> str, out byte value) => byte.TryParse(str, out value); }

    struct ParserF32 : IParser<float> { public bool TryParse(ReadOnlySpan<char> str, out float value) => float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out value); }
    struct ParserF64 : IParser<double> { public bool TryParse(ReadOnlySpan<char> str, out double value) => double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out value); }
    struct ParserDecimal : IParser<decimal> { public bool TryParse(ReadOnlySpan<char> str, out decimal value) => decimal.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out value); }

    struct ParserBool : IParser<bool> { public bool TryParse(ReadOnlySpan<char> str, out bool value) => bool.TryParse(str, out value); }
    struct ParserDateTime : IParser<DateTime> { public bool TryParse(ReadOnlySpan<char> str, out DateTime value) => DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out value); }

    struct ParserString : IParser<string>
    {
        public bool TryParse(ReadOnlySpan<char> str, out string value)
        {
            value = str.ToString();
            return true;
        }
    }

    struct ParserChar : IParser<char>
    {
        public bool TryParse(ReadOnlySpan<char> str, out char value)
        {
            value = str.Length == 1 ? str[0] : default;
            return str.Length == 1;
        }
    }

    public delegate bool CustomParseDelegate<T>(ReadOnlySpan<char> str, out T value);

    static class Converter<T>
    {
        #pragma warning disable 649
        public static CustomParseDelegate<T> Custom;
        #pragma warning restore 649

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(ReadOnlySpan<char> str, out T value)
        {
            if (Custom != null)
                return Custom(str, out value);

            // main types
            if (typeof(T) == typeof(string)) return Parse<string, ParserString>(str, out value);
            if (typeof(T) == typeof(bool) || typeof(T) == typeof(bool?)) return Parse<bool, ParserBool>(str, out value);
            if (typeof(T) == typeof(char) || typeof(T) == typeof(char?)) return Parse<char, ParserChar>(str, out value);
            // numbers
            if (typeof(T) == typeof(long) || typeof(T) == typeof(long?)) return Parse<long, ParserI64>(str, out value);
            if (typeof(T) == typeof(ulong) || typeof(T) == typeof(ulong?)) return Parse<ulong, ParserU64>(str, out value);
            if (typeof(T) == typeof(int) || typeof(T) == typeof(int?)) return Parse<int, ParserI32>(str, out value);
            if (typeof(T) == typeof(uint) || typeof(T) == typeof(uint?)) return Parse<uint, ParserU32>(str, out value);
            if (typeof(T) == typeof(short) || typeof(T) == typeof(short?)) return Parse<short, ParserI16>(str, out value);
            if (typeof(T) == typeof(ushort) || typeof(T) == typeof(ushort?)) return Parse<ushort, ParserU16>(str, out value);
            if (typeof(T) == typeof(sbyte) || typeof(T) == typeof(sbyte?)) return Parse<sbyte, ParserI8>(str, out value);
            if (typeof(T) == typeof(byte) || typeof(T) == typeof(byte?)) return Parse<byte, ParserU8>(str, out value);
            // floats
            if (typeof(T) == typeof(float) || typeof(T) == typeof(float?)) return Parse<float, ParserF32>(str, out value);
            if (typeof(T) == typeof(double) || typeof(T) == typeof(double?)) return Parse<double, ParserF64>(str, out value);
            if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?)) return Parse<decimal, ParserDecimal>(str, out value);
            // others
            if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?)) return Parse<DateTime, ParserDateTime>(str, out value);

            return ThrowNotSupportedType(out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool Parse<TVal, TParser>(ReadOnlySpan<char> str, out T value)
            where TParser : struct, IParser<TVal>
        {
            TParser parser = default;
            bool parsed = parser.TryParse(str, out var temp);
            value = parsed ? (T)(object)temp : default;
            return parsed;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool ThrowNotSupportedType(out T value) => throw new NotSupportedException($"The '{typeof(T).Name}' is not supported");
    }
}