namespace System.TinyCommandLine.Implementation
{
    static class ExceptionHelper
    {
        public static Exception ArgumentNotSpecified(string name) => throw new InvalidSyntaxException($"Argument {name} not specified.");
        public static Exception OptionNotSpecified(string name) => throw new InvalidSyntaxException($"Option {name} not specified.");
        public static Exception InvalidOptionType(string name, string type) => throw new InvalidSyntaxException($"Option --{name} must be a {type}.");
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