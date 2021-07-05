using System;
using System.Collections.Generic;
using Tiny.CommandLine.Implementation;

namespace Tiny.CommandLine
{
    public delegate void CommandConfigurator(CommandBuilder builder);

    public delegate void OptionConfigurator<T>(OptionBuilder<T> builder);

    public readonly ref struct CommandBuilder
    {
        readonly Parser _parser;

        internal CommandBuilder(Parser parser) => _parser = parser;

        public CommandBuilder Command(string name, CommandConfigurator configure)
        {
            _parser.Command(name, configure);
            return this;
        }

        public CommandBuilder Argument<T>(out T value, OptionConfigurator<T> configure = null)
        {
            value = _parser.Argument(configure);
            return this;
        }

        public CommandBuilder ArgumentList<T>(out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
        {
            value = _parser.ArgumentList(configure);
            return this;
        }

        public CommandBuilder Option<T>(char shortName, string longName, out T value, OptionConfigurator<T> configure = null)
        {
            value = _parser.Option(shortName, longName, configure);
            return this;
        }

        public CommandBuilder OptionList<T>(char shortName, string longName, out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
        {
            value = _parser.OptionList(shortName, longName, configure);
            return this;
        }

        public CommandBuilder Check(Func<bool> predicate, string message)
        {
            _parser.Check(predicate, message);
            return this;
        }

        public CommandBuilder HelpText(string text)
        {
            _parser.HelpText(text);
            return this;
        }

        public void Handler(Action handler)
        {
            _parser.Handler(handler);
        }
    }
}