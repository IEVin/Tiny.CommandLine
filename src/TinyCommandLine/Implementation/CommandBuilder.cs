using System;
using System.Collections.Generic;

namespace Tiny.CommandLine.Implementation
{
    public delegate void CommandConfigurator(CommandBuilder builder);

    public delegate void OptionConfigurator<T>(OptionBuilder<T> builder);

    class State
    {
        public string SubCommandName;
        public CommandConfigurator SubCommandHandler;
        public Action Handler;
        public string ErrReason;
        public bool IsFinished;
        public bool IsHelpRequired;
        public bool IsHelpChecked;
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

        bool CheckState<T>(out T valueDefault, OptionConfigurator<T> configure, bool isList, char shortName = '\0', string longName = null)
        {
            valueDefault = default;
            if (_help != null)
            {
                _help.AddOption(shortName, longName, configure, isList);
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
                _state.SubCommandHandler = configure;
                _state.SubCommandName = name;
                _state.IsFinished = true;
                _state.IsHelpChecked = true;
            }

            return this;
        }

        public CommandBuilder Argument<T>(out T value, OptionConfigurator<T> configure = null)
        {
            CheckIsHelpRequired();

            if (!CheckState(out value, configure, false))
                return this;

            if (ArgumentInternal(out value, configure))
                return this;

            var opState = GetOptionState(configure);

            value = opState.IsRequired
                ? SetErrorArgumentRequired(opState)
                : opState.DefaultValue;

            return this;
        }

        public CommandBuilder ArgumentList<T>(out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
        {
            CheckIsHelpRequired();

            if (!CheckState(out value, configure, true))
                return this;

            var list = new List<T>(_tokens.RemainingItemsCount);
            while (ArgumentInternal(out T item, configure))
            {
                list.Add(item);
            }

            if (list.Count > 0)
            {
                value = list;
                return this;
            }

            var opState = GetOptionState(configure);

            value = opState.IsRequired
                ? SetErrorArgumentRequired(opState)
                : opState.DefaultValue;

            return this;
        }

        public CommandBuilder Option<T>(char shortName, string longName, out T value, OptionConfigurator<T> configure = null)
        {
            if (!CheckState(out value, configure, false, shortName, longName))
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

            if (optionIndex >= 0)
            {
                value = GetOptionValue<T>(optionIndex, optionLength);
                return this;
            }

            var opState = GetOptionState(configure);

            value = opState.IsRequired
                ? SetErrorOptionRequired<T>(shortName, longName)
                : opState.DefaultValue;

            return this;
        }

        public CommandBuilder OptionList<T>(char shortName, string longName, out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
        {
            if (!CheckState(out value, configure, true, shortName, longName))
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

            if (result.Count > 0)
            {
                value = result;
                return this;
            }

            var opState = GetOptionState(configure);

            value = opState.IsRequired
                ? SetErrorOptionRequired<IReadOnlyList<T>>(shortName, longName)
                : opState.DefaultValue;

            return this;
        }

        void CheckIsHelpRequired()
        {
            if (_state == null || _state.IsHelpChecked)
                return;

            _state.IsHelpChecked = true;

            Option('h', "help", out _state.IsHelpRequired);

            if (_state.IsHelpRequired)
                _state.IsFinished = true;
        }

        OptionState<T> GetOptionState<T>(OptionConfigurator<T> configure)
        {
            var optionState = new OptionState<T>();
            configure?.Invoke(new OptionBuilder<T>(optionState));

            return optionState;
        }

        T SetErrorArgumentRequired<T>(OptionState<T> optionState)
        {
            var name = optionState.ValueName ?? "argument";
            SetError($"Argument {name} is not specified.");
            return default;
        }

        T SetErrorOptionRequired<T>(char shortName, string longName)
        {
            var name = longName != null ? "--" + longName : "-" + shortName;
            SetError($"Option {name} is not specified.");
            return default;
        }

        bool ArgumentInternal<T, TConf>(out T value, OptionConfigurator<TConf> configure)
        {
            int index = _tokens.GetNextIndex();
            if (index < 0)
            {
                value = default;
                return false;
            }

            _tokens.MarkAsUsed(index);

            var valStr = _tokens[index];

            if (Converter<T>.TryParse(valStr, out value))
                return true;

            var optionState = new OptionState<TConf>();
            configure?.Invoke(new OptionBuilder<TConf>(optionState));

            var name = optionState.ValueName ?? "argument";
            SetParseError(name);
            return false;
        }

        T GetOptionValue<T>(int index, int length)
        {
            var token = _tokens[index];
            if (length < token.Length)
            {
                var str = token.AsSpan(length + 1);

                if (Converter<T>.TryParse(str, out var value))
                    return value;

                SetParseError(token.Substring(0, length));
                return default;
            }

            if (typeof(T) == typeof(bool))
                return (T) (object) true;

            if (index + 1 < _tokens.Count)
            {
                var valueStr = _tokens[index + 1];
                if (Converter<T>.TryParse(valueStr, out var value))
                    return value;

                SetParseError(valueStr);
                return default;
            }

            SetError($"Option {token} value expected.");
            return default;
        }

        void SetParseError(string optionName) => SetError($"Option {optionName} can't be parsed.");

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
            if (_help != null || _state.IsFinished)
                return;

            if (_state.SubCommandHandler != null)
                return;

            _state.Handler = handler;
        }
    }
}