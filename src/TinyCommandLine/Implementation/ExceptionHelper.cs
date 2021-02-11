namespace System.TinyCommandLine.Implementation
{
    static class ExceptionHelper
    {
        public static Exception ArgumentNotSpecified() => new InvalidSyntaxException("Argument not specified.");
        public static Exception OptionNotSpecified(string name) => new InvalidSyntaxException($"Option --{name} not specified.");
        public static Exception InvalidOptionType(string name, string type) => new InvalidSyntaxException($"Option --{name} must be a {type}.");
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