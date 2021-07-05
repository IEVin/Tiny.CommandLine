using System;
using System.Collections.Generic;

namespace Tiny.CommandLine.Implementation
{
    readonly ref struct Parser
    {
        public const char NoShortName = '\0';
        public const string NoLongName = null;

        readonly HelpCollector _help;
        readonly TokenCollection _tokens;
        readonly CommandState _state;

        internal Parser(HelpCollector help) : this(null, null) => _help = help;

        internal Parser(TokenCollection tokens, CommandState state)
        {
            _help = null;
            _tokens = tokens;
            _state = state;
        }

        bool CheckState<T>(OptionConfigurator<T> configure, bool isList, char shortName, string longName)
        {
            if (_help != null)
            {
                _help.AddOption(shortName, longName, configure, isList);
                return false;
            }

            return !_state.IsFinished;
        }

        public void Command(string name, CommandConfigurator configure)
        {
            if (_help != null)
            {
                _help.AddCommand(name, configure);
                return;
            }

            if (_state.IsFinished)
                return;

            var index = _tokens.GetNextIndex();
            if (index >= 0 && _tokens[index] == name)
            {
                _tokens.MarkAsUsed(index);
                _state.SubCommandHandler = configure;
                _state.SubCommandName = name;
                _state.IsFinished = true;
                _state.IsHelpChecked = true;
            }
        }

        public T Argument<T>(OptionConfigurator<T> configure)
        {
            CheckIsHelpRequired();

            if (!CheckState(configure, false, NoShortName, NoLongName))
                return default;

            if (ArgumentInternal(out T value, configure))
                return value;

            var opState = GetOptionState(configure);

            return opState.IsRequired
                ? SetErrorArgumentRequired(opState)
                : opState.DefaultValue;
        }

        public IReadOnlyList<T> ArgumentList<T>(OptionConfigurator<IReadOnlyList<T>> configure)
        {
            CheckIsHelpRequired();

            if (!CheckState(configure, true, NoShortName, NoLongName))
                return default;

            var list = new List<T>(_tokens.RemainingItemsCount);
            while (ArgumentInternal(out T item, configure))
            {
                list.Add(item);
            }

            if (list.Count > 0)
                return list;

            var opState = GetOptionState(configure);

            return opState.IsRequired
                ? SetErrorArgumentRequired(opState)
                : opState.DefaultValue;
        }

        public T Option<T>(char shortName, string longName, OptionConfigurator<T> configure)
        {
            if (!CheckState(configure, false, shortName, longName))
                return default;

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
                return GetOptionValue<T>(optionIndex, optionLength);

            var opState = GetOptionState(configure);

            return opState.IsRequired
                ? SetErrorOptionRequired<T>(shortName, longName)
                : opState.DefaultValue;
        }

        public IReadOnlyList<T> OptionList<T>(char shortName, string longName, OptionConfigurator<IReadOnlyList<T>> configure)
        {
            if (!CheckState(configure, true, shortName, longName))
                return default;

            bool isFlag = typeof(T) == typeof(bool);

            var itr = _tokens.IterateOptions(shortName, longName, isFlag);

            var result = new List<T>();
            while (itr.TryMoveNext(out var index, out var length))
            {
                var optionValue = GetOptionValue<T>(index, length);
                if (_state.IsFinished)
                    return default;

                result.Add(optionValue);
            }

            if (result.Count > 0)
                return result;

            var opState = GetOptionState(configure);

            return opState.IsRequired
                ? SetErrorOptionRequired<IReadOnlyList<T>>(shortName, longName)
                : opState.DefaultValue;
        }


        public void CheckIsHelpRequired()
        {
            if (_state == null || _state.IsHelpChecked)
                return;

            _state.IsHelpRequired = Option<bool>('h', "help", null);
            _state.IsHelpChecked = true;

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

        public void Check(Func<bool> predicate, string message)
        {
            if (_help != null || _state.IsFinished)
                return;

            if (predicate())
                return;

            SetError(message);
        }

        public void HelpText(string text)
        {
            _help?.HelpText(text);
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