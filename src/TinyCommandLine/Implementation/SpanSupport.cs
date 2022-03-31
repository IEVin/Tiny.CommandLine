namespace Tiny.CommandLine.Implementation
{
#if !(NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER)
    /* It's not a complete implementation of ReadOnlySpan<T> but small wrapper over string to support old framework versions */
    public readonly struct ReadOnlySpan<T>
    {
        readonly string _str;
        ReadOnlySpan(string str) => _str = str;

        public char this[int index] => _str[index];
        public int Length => _str?.Length ?? 0;

        public static implicit operator ReadOnlySpan<T>(string str) => new ReadOnlySpan<T>(str);
        public static implicit operator string(ReadOnlySpan<T> span) => span.ToString();

        public override string ToString() => _str ?? string.Empty;
    }

    public static class SpanExt
    {
        public static ReadOnlySpan<char> AsSpan(this string str, int start) => str.Substring(start);
        public static ReadOnlySpan<char> AsSpan(this string str, int start, int length) => str.Substring(start, length);
    }
#endif
}