using System.Collections.Generic;

namespace System.TinyCommandLine.Implementation
{
    public delegate void CommandConfigurator(CommandBuilder builder);

    public delegate void OptionConfigurator<T>(OptionBuilder<T> builder);

    class State
    {
        public CommandConfigurator SubCommand;
        public Action Handler;
        public string ErrReason;
        public bool IsFinished;
    }

    public readonly ref struct CommandBuilder
    {
        readonly TokenCollection _tokens;
        readonly HelpCollector _help;
        readonly State _state;

        internal CommandBuilder(HelpCollector help) : this(null, null) => _help = help;

        internal CommandBuilder(TokenCollection tokens, State state)
        {
            _help = null;
            _tokens = tokens;
            _state = state;
        }

        bool CheckState<T>(out T valueDefault, OptionConfigurator<T> configure, char shortName='\0', string longName = null)
        {
            valueDefault = default;
            if (_help != null)
            {
                _help.AddOption(shortName, longName, configure);
                return false;
            }

            if (_state.IsFinished)
                return false;

            return true;
        }

        public CommandBuilder Command(string name, CommandConfigurator configure)
        {
            if (_help != null)
            {
                _help.AddCommand(name, configure);
                return this;
            }

            if (_state.IsFinished)
                return this;

            var index = _tokens.GetNextIndex();
            if (index >= 0 && _tokens[index] == name)
            {
                _tokens.MarkAsUsed(index);
                _state.SubCommand = configure;
                _state.IsFinished = true;
            }

            return this;
        }

        public CommandBuilder Argument<T>(out T value, OptionConfigurator<T> configure = null)
        {
            if (!CheckState(out value, configure))
                return this;

            if (ArgumentInternal(out value))
                return this;

            value = GetDefaultValue(configure);
            return this;
        }

        public CommandBuilder ArgumentList<T>(out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
        {
            if (!CheckState(out value, configure))
                return this;

            var list = new List<T>(_tokens.RemainingItemsCount);
            while (ArgumentInternal(out T item))
            {
                list.Add(item);
            }

            value = list.Count > 0
                ? list
                : GetDefaultValue(configure);

            return this;
        }

        public CommandBuilder Option<T>(char shortName, string longName, out T value, OptionConfigurator<T> configure = null)
        {
            if (!CheckState(out value, configure, shortName, longName))
                return this;

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
                : GetDefaultValue(configure);

            return this;
        }

        public CommandBuilder OptionList<T>(char shortName, string longName, out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
        {
            if (!CheckState(out value, configure, shortName, longName))
                return this;

            bool isFlag = typeof(T) == typeof(bool);

            var itr = _tokens.IterateOptions(shortName, longName, isFlag);

            var result = new List<T>();
            while (itr.TryMoveNext(out var index, out var length))
            {
                var optionValue = GetOptionValue<T>(index, length);
                if (_state.IsFinished)
                {
                    value = default;
                    return this;
                }

                result.Add(optionValue);
            }

            value = result.Count > 0
                ? result
                : GetDefaultValue(configure);

            return this;
        }

        T GetDefaultValue<T>(OptionConfigurator<T> configure)
        {
            var optionState = new OptionState<T>();
            configure?.Invoke(new OptionBuilder<T>(optionState));

            if (optionState.IsRequired)
            {
                // TODO: Add option name
                SetError($"Option {null} not specified.");
                return default;
            }

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

            // TODO: Add option name
            if (Converter<T>.TryParse(valStr, null, out value, out var error))
                return true;

            SetError(error);
            return false;
        }

        T GetOptionValue<T>(int index, int length)
        {
            var token = _tokens[index];
            if (length < token.Length)
            {
                var optionName = token.AsSpan(0, length);
                var str = token.AsSpan(length + 1);

                if (Converter<T>.TryParse(str, optionName, out var value, out var error))
                    return value;

                SetError(error);
                return default;
            }

            if (typeof(T) == typeof(bool))
                return (T) (object) true;

            if (index + 1 < _tokens.Count)
            {
                var valueStr = _tokens[index + 1];
                if (Converter<T>.TryParse(valueStr, token, out var value, out var error))
                    return value;

                SetError(error);
                return default;
            }

            SetError($"Option {token} value expected.");
            return default;
        }

        void SetError(string reason)
        {
            _state.ErrReason = reason;
            _state.IsFinished = true;
        }

        public CommandBuilder Check(Func<bool> predicate, string message)
        {
            if (_help != null || _state.IsFinished)
                return this;

            if (predicate())
                return this;

            SetError(message);
            return this;
        }

        public CommandBuilder HelpText(string text)
        {
            _help?.HelpText(text);
            return this;
        }

        public void Handler(Action handler)
        {
            if(_help != null || _state.IsFinished)
                return;

            if (_state.SubCommand == null)
                _state.Handler = handler;
        }
    }
}