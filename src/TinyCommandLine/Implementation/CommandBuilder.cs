using System.Collections.Generic;

namespace System.TinyCommandLine.Implementation
{
    public delegate void CommandConfigurator(CommandBuilder builder);

    public delegate void OptionConfigurator<T>(OptionBuilder<T> builder);

    class State
    {
        public CommandConfigurator Command;
        public Action Handler;
        public int StartIndex;
        public int Count;
        public bool IsHelpRequired;
    }


    public readonly ref struct CommandBuilder
    {
        readonly TokenCollection _tokens;
        readonly HelpGenerator _helpGen;
        readonly State _state;

        internal CommandBuilder(HelpGenerator helpGen) : this(null, null) => _helpGen = helpGen;

        internal CommandBuilder(TokenCollection tokens, State state)
        {
            _helpGen = null;
            _tokens = tokens;
            _state = state;
        }

        public CommandBuilder Command(string name, CommandConfigurator configure)
        {
            if (_helpGen != null)
            {
                _helpGen.AddCommand(name, configure);
                return this;
            }

            var index = _tokens.GetIndex(name, _state.StartIndex, _state.Count);
            if (index >= 0)
            {
                _state.Count = index - _state.StartIndex;
                _state.Command = configure;
            }

            // TODO: Add check for duplicate commands
            return this;
        }

        public CommandBuilder Option<T>(char shortName, string longName, out T value, OptionConfigurator<T> configure = null)
            => OptionsInternal<T, T>(shortName, longName, out value, configure, x => x[x.Count - 1]);

        public CommandBuilder Argument<T>(out T value, OptionConfigurator<T> configure = null)
            => ArgumentInternal<T, T>(out value, configure, x => x[x.Count - 1]);

        public CommandBuilder OptionList<T>(char shortName, string longName, out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
            => OptionsInternal<IReadOnlyList<T>, T>(shortName, longName, out value, configure, x => x);

        public CommandBuilder ArgumentList<T>(out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
            => ArgumentInternal<IReadOnlyList<T>, T>(out value, configure, x => x);

        CommandBuilder ArgumentInternal<T, TItem>(out T value, OptionConfigurator<T> configure, Func<List<TItem>, T> func)
        {
            // TODO: Add stage check
            return OptionsInternal('\0', null, out value, configure, func);
        }

        CommandBuilder OptionsInternal<T, TItem>(char shortName, string longName, out T value, OptionConfigurator<T> configure, Func<List<TItem>, T> func)
        {
            if (_helpGen != null)
            {
                _helpGen.AddOption(shortName, longName, configure);
                value = default;
                return this;
            }

            var result = _tokens.GetValues<TItem>(shortName, longName, _state.StartIndex, _state.Count);
            if (result.Count != 0)
            {
                value = func(result);
                return this;
            }

            var optionState = new OptionState<T>();
            configure?.Invoke(new OptionBuilder<T>(optionState));

            if (optionState.IsRequired)
                throw ExceptionHelper.OptionNotSpecified(longName);

            value = optionState.DefaultValue;
            return this;
        }

        public CommandBuilder Check(Func<bool> predicate, string message)
        {
            if (_helpGen != null)
                return this;

            if (predicate())
                return this;

            throw new CheckFailedException(message);
        }

        public CommandBuilder HelpText(string text)
        {
            _helpGen?.AddDesc(text);
            return this;
        }

        public void Handler(Action handler)
        {
            if (_helpGen != null)
                return;

            if (_state.Command == null)
            {
                _state.Handler = handler;
            }
        }
    }
}