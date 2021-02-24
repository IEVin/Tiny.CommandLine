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
                ? (IReadOnlyList<T>) list
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

            var valueOffset = 0;
            var valueIndex = -1;

            bool isFlag = typeof(T) == typeof(bool);
            _tokens.EnumerateOptions(shortName, longName, isFlag, (index, offset) =>
            {
                valueIndex = index;
                valueOffset = offset;
            });

            if (valueIndex >= 0)
            {
                value = GetOptionValue<T>(valueIndex, valueOffset);
                return this;
            }

            value = GetDefaultValueOrThrowException(configure);
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

            var values = new List<(int, int)>();
            _tokens.EnumerateOptions(shortName, longName, isFlag, (index, offset) => { values.Add((index, offset)); });

            var list = new List<T>(values.Count);
            foreach (var (index, offset) in values)
            {
                var optionValue = GetOptionValue<T>(index, offset);
                list.Add(optionValue);
            }

            value = list.Count > 0
                ? (IReadOnlyList<T>) list
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

        T GetOptionValue<T>(int valueIndex, int valueOffset)
        {
            if (typeof(T) == typeof(bool) && valueOffset == 0)
            {
                return Converter<T>.Cast(true);
            }

            // TODO: remove this hack
            var optionName = valueOffset > 0
                ? _tokens[valueIndex].Remove(valueOffset - 1)
                : _tokens[valueIndex - 1];

            if (valueIndex >= _tokens.Count)
                throw ExceptionHelper.OptionHasNoValue(optionName);

            string str = _tokens[valueIndex].Substring(valueOffset);
            return Converter<T>.Parse(str, optionName);
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