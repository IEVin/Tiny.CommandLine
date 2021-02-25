using System.Collections.Generic;

namespace System.TinyCommandLine.Implementation
{
    public delegate void CommandConfigurator(CommandBuilder builder);

    public delegate void OptionConfigurator<T>(OptionBuilder<T> builder);

    class State
    {
        public CommandConfigurator SubCommand;
        public Action Handler;
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
            if (_state.SubCommand != null)
                return this;

            if (_helpGen != null)
            {
                _helpGen.AddCommand(name, configure);
                return this;
            }

            var index = _tokens.GetNextIndex();
            if (index >= 0 && _tokens[index] == name)
            {
                _tokens.MarkAsUsed(index);
                _state.SubCommand = configure;
            }

            return this;
        }

        public CommandBuilder Argument<T>(out T value, OptionConfigurator<T> configure = null)
        {
            if (_state.SubCommand != null)
            {
                value = default;
                return this;
            }

            if (_helpGen != null)
            {
                _helpGen.AddArgument(configure);
                value = default;
                return this;
            }

            if (ArgumentInternal(out value))
                return this;

            value = GetDefaultValueOrThrowException(configure);
            return this;
        }

        public CommandBuilder ArgumentList<T>(out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
        {
            if (_state.SubCommand != null)
            {
                value = default;
                return this;
            }

            if (_helpGen != null)
            {
                _helpGen.AddArgument(configure);
                value = default;
                return this;
            }

            var list = new List<T>(_tokens.RemainingItemsCount);
            while (ArgumentInternal(out T item))
            {
                list.Add(item);
            }

            value = list.Count > 0
                ? list
                : GetDefaultValueOrThrowException(configure);

            return this;
        }

        public CommandBuilder Option<T>(char shortName, string longName, out T value, OptionConfigurator<T> configure = null)
        {
            if (_state.SubCommand != null)
            {
                value = default;
                return this;
            }

            if (_helpGen != null)
            {
                _helpGen.AddOption(shortName, longName, configure);
                value = default;
                return this;
            }

            bool isFlag = typeof(T) == typeof(bool);
            var itr = _tokens.IterateOptions(shortName, longName, isFlag);

            int optionIndex = int.MinValue;
            int optionLength = int.MinValue;

            while (itr.TryMoveNext(out var index, out var offset))
            {
                optionIndex = index;
                optionLength = offset;
            }

            value = optionIndex >= 0
                ? GetOptionValue<T>(optionIndex, optionLength)
                : GetDefaultValueOrThrowException(configure);

            return this;
        }

        public CommandBuilder OptionList<T>(char shortName, string longName, out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
        {
            if (_state.SubCommand != null)
            {
                value = default;
                return this;
            }

            if (_helpGen != null)
            {
                _helpGen.AddOption(shortName, longName, configure);
                value = default;
                return this;
            }

            bool isFlag = typeof(T) == typeof(bool);

            var itr = _tokens.IterateOptions(shortName, longName, isFlag);

            var result = new List<T>();
            while (itr.TryMoveNext(out var index, out var length))
            {
                var optionValue = GetOptionValue<T>(index, length);
                result.Add(optionValue);
            }

            value = result.Count > 0
                ? result
                : GetDefaultValueOrThrowException(configure);

            return this;
        }

        T GetDefaultValueOrThrowException<T>(OptionConfigurator<T> configure)
        {
            var optionState = new OptionState<T>();
            configure?.Invoke(new OptionBuilder<T>(optionState));

            if (optionState.IsRequired)
                ExceptionHelper.OptionNotSpecified(null); // TODO: Add option name

            return optionState.DefaultValue;
        }

        bool ArgumentInternal<T>(out T value)
        {
            int index = _tokens.GetNextIndex();
            if (index < 0)
            {
                value = default;
                return false;
            }

            _tokens.MarkAsUsed(index);

            var valStr = _tokens[index];
            value = Converter<T>.Parse(valStr, string.Empty);
            return true;
        }

        T GetOptionValue<T>(int index, int length)
        {
            var token = _tokens[index];
            if (token.Length != length)
            {
                var optionName = token.Remove(length);
                var str = token.Substring(length + 1);
                return Converter<T>.Parse(str, optionName);
            }

            if (typeof(T) == typeof(bool))
                return Converter<T>.Cast(true);

            if (index + 1 >= _tokens.Count)
                throw ExceptionHelper.OptionHasNoValue(token);

            var valueStr = _tokens[index + 1];
            return Converter<T>.Parse(valueStr, token);
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

            if (_state.SubCommand != null)
                return;

            _state.Handler = handler;
        }
    }
}