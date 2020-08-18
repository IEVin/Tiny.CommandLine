using System.Runtime.CompilerServices;

// TODO:
// - usage for commands
// - commands appearance as options (like --help or --version)
// - default value for property specified as flag
// - argument value name
// - enum support
// - print syntax


namespace System.TinyCommandLine
{
    using Implementation;

    public delegate void CommandConfigurator(CommandBuilder builder);

    public delegate void CommandConfigurator<in T>(T options, CommandBuilder builder);

    public delegate void OptionConfigurator<T>(OptionBuilder<T> builder);

    public class CommandLineParser
    {
        public CommandLineParser Command(string name, CommandConfigurator configure) => throw new NotImplementedException(name);
        public CommandLineParser ErrorHandler(Func<string> handler) => throw new NotImplementedException();
        public void Run() => throw new NotImplementedException();
    }

    namespace Implementation
    {
        public ref struct CommandBuilder
        {
            public CommandBuilder Option<T>(char shortName, string longName, out T value, OptionConfigurator<T> configure = null)
                => throw new NotImplementedException();

            public CommandBuilder Check(Func<bool> predicate, string message) => throw new NotImplementedException();

            public CommandBuilder HelpText(string message) => throw new NotImplementedException();

            public void Invoke(Action handler) => throw new NotImplementedException();
        }

        public ref struct OptionBuilder<T>
        {
            public OptionBuilder<T> HelpText(string str) => throw new NotImplementedException();
            public OptionBuilder<T> Hidden() => throw new NotImplementedException();
            public OptionBuilder<T> Default(T value) => throw new NotImplementedException();
            public OptionBuilder<T> Required() => throw new NotImplementedException();
        }
    }

    public static class CommandLineParserExtensions
    {
        const string NoCommand = null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandLineParser Command(this CommandLineParser parser, CommandConfigurator configure)
            => parser.Command(NoCommand, configure);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandLineParser Command<T>(this CommandLineParser parser, Func<T> ctor, CommandConfigurator<T> configure) where T : new()
            => parser.Command(b => configure(ctor(), b));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandLineParser Command<T>(this CommandLineParser parser, string name, Func<T> ctor, CommandConfigurator<T> configure)
            => parser.Command(name, b => configure(ctor(), b));
    }

    public static class CommandBuilderExtensions
    {
        const char NoShortName = '\0';
        const string NoLongName = null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder CreateOptions<T>(this CommandBuilder builder, T optionsDefaultObject, out T options)
        {
            options = optionsDefaultObject;
            return builder;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invoke<T>(this CommandBuilder builder, Action<T> handler, T options)
            => builder.Invoke(() => handler(options));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder Option<T>(this CommandBuilder builder, string longName, out T value, string helpText)
            => builder.Option(NoShortName, longName, out value, b => b.HelpText(helpText));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder Option<T>(this CommandBuilder builder, string longName, out T value, OptionConfigurator<T> configure = null)
            => builder.Option(NoShortName, longName, out value, configure);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder Option<T>(this CommandBuilder builder, char shortName, out T value, string helpText)
            => builder.Option(shortName, NoLongName, out value, b => b.HelpText(helpText));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder Option<T>(this CommandBuilder builder, char shortName, out T value, OptionConfigurator<T> configure)
            => builder.Option(shortName, NoLongName, out value, configure);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder Option<T>(this CommandBuilder builder, out T value, string helpText)
            => builder.Option(NoShortName, NoLongName, out value, b => b.HelpText(helpText));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder Option<T>(this CommandBuilder builder, out T value, OptionConfigurator<T> configure = null)
            => builder.Option(NoShortName, NoLongName, out value, configure);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder Option<T>(this CommandBuilder builder, char shortName, string longName, out T value, string helpText)
            => builder.Option(shortName, longName, out value, b => b.HelpText(helpText));
    }

    namespace Extended
    {
        public static class CommandLineParserExtensions
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static CommandLineParser Command<T>(this CommandLineParser parser, CommandConfigurator<T> configure) where T : new()
                => parser.Command(b => configure(new T(), b));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static CommandLineParser Command<T>(this CommandLineParser parser, string name, CommandConfigurator<T> configure) where T : new()
                => parser.Command(name, b => configure(new T(), b));
        }

        public static class CommandBuilderExtensions
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static CommandBuilder CreateOptions<T>(this CommandBuilder builder, out T options)
                where T : new()
                => builder.CreateOptions(new T(), out options);
        }
    }
}