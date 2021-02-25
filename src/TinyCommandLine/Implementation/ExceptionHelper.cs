namespace System.TinyCommandLine.Implementation
{
    static class ExceptionHelper
    {
        public static Exception ArgumentNotSpecified(string name) => throw new InvalidSyntaxException($"Argument {name} not specified.");
        public static Exception OptionNotSpecified(string name) => throw new InvalidSyntaxException($"Option {name} not specified.");
        public static Exception InvalidOptionType(ReadOnlySpan<char> name, string type) => throw new InvalidSyntaxException($"Option {name.ToString()} must be a {type}.");
        public static Exception OptionTypeNotSupported(ReadOnlySpan<char> name) => throw new NotSupportedException($"The type of option '{name.ToString()}' is not supported");
        public static Exception OptionHasNoValue(string name) => throw new InvalidSyntaxException($"Option {name} value expected.");
    }

    public class InvalidSyntaxException : Exception
    {
        public InvalidSyntaxException(string message) : base(message) { }
    }

    public class CheckFailedException : Exception
    {
        public CheckFailedException(string message) : base(message) { }
    }
}