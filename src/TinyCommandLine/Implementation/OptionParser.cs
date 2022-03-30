using System;
using System.Collections.Generic;

namespace Tiny.CommandLine.Implementation
{
    class OptionParser
    {
        readonly TokenCollection _tokens;
        string _error;

        public static bool IsFlag<T>() => typeof(T) == typeof(bool) || typeof(T) == typeof(bool?);

        public OptionParser(string[] args)
        {
            _tokens = TokenCollection.Tokenize(args);
        }

        public bool HasError => _error != null;
        public string ErrorReason => _error;

        public bool Command(string name)
        {
            var index = _tokens.GetNextIndex();
            if (index < 0 || _tokens[index] != name)
                return false;

            _tokens.MarkAsUsed(index);
            return true;
        }

        public T Option<T>(char alias, string name, Func<T> valueDefault, bool required)
        {
            var itr = _tokens.IterateOptions(alias, name);

            int optionIndex = 0;
            int optionLength = 0;
            ReadOnlySpan<char> valueToken = null;

            while (itr.TryMoveNext(out var index, out var length))
            {
                if (!TryGetOptionValueToken<T>(index, length, out valueToken))
                    return default;

                optionIndex = index;
                optionLength = length;
            }

            if (optionLength != 0)
                return GetOptionValue<T>(valueToken, optionIndex, optionLength);

            return required
                ? SetErrorOptionRequired<T>(alias, name)
                : GetDefault(valueDefault);
        }

        public IReadOnlyList<T> OptionList<T>(char alias, string name, Func<IReadOnlyList<T>> valueDefault, bool required)
        {
            var itr = _tokens.IterateOptions(alias, name);

            var result = new List<T>();
            while (itr.TryMoveNext(out var index, out var length))
            {
                if (!TryGetOptionValueToken<T>(index, length, out var valueToken))
                    return default;

                var optionValue = GetOptionValue<T>(valueToken, index, length);
                if (_error != null)
                    return default;

                result.Add(optionValue);
            }

            if (result.Count > 0)
                return result;

            return required
                ? SetErrorOptionRequired<IReadOnlyList<T>>(alias, name)
                : GetDefault(valueDefault);
        }

        public T Argument<T>(Func<T> valueDefault, bool required, string valueName)
        {
            if (TryGetArgument(out T value, valueName))
                return value;

            return required
                ? SetErrorArgumentRequired<T>(valueName)
                : GetDefault(valueDefault);
        }

        public IReadOnlyList<T> ArgumentList<T>(Func<IReadOnlyList<T>> valueDefault, bool required, string valueName)
        {
            var list = new List<T>(_tokens.RemainingItemsCount);
            while (TryGetArgument(out T item, valueName))
            {
                list.Add(item);
            }

            if (list.Count > 0)
                return list;

            return required
                ? SetErrorArgumentRequired<IReadOnlyList<T>>(valueName)
                : GetDefault(valueDefault);
        }

        bool TryGetArgument<T>(out T value, string valueName)
        {
            int index = _tokens.GetNextIndex();
            if (index < 0)
            {
                value = default;
                return false;
            }

            _tokens.MarkAsUsed(index);

            if (Converter<T>.TryParse(_tokens[index], out value))
                return true;

            SetParseError(valueName ?? "argument");
            return false;
        }

        public bool TryGetOptionValueToken<T>(int index, int length, out ReadOnlySpan<char> value)
        {
            value = default;

            string token = _tokens[index];
            if (length < token.Length)
            {
                value = token.AsSpan(length + 1);
                return true;
            }

            if (IsFlag<T>())
                return true;

            if (index + 1 >= _tokens.Count || _tokens.IsUsed(index + 1))
            {
                SetError($"Option {token.Substring(0, length)} value expected.");
                return false;
            }

            _tokens.MarkAsUsed(index + 1);
            value = _tokens[index + 1];
            return true;
        }

        T GetOptionValue<T>(ReadOnlySpan<char> valueToken, int optionIndex, int optionLength)
        {
            if (IsFlag<T>() && valueToken.IsEmpty)
                return (T)(object)true;

            if (Converter<T>.TryParse(valueToken, out var value))
                return value;

            SetParseError(_tokens[optionIndex].Remove(optionLength));
            return default;
        }

        void SetParseError(string optionName) => SetError($"Option {optionName} can't be parsed.");

        T SetErrorOptionRequired<T>(char shortName, string longName)
        {
            var name = longName != null ? "--" + longName : "-" + shortName;
            SetError($"Option {name} is not specified.");
            return default;
        }

        T SetErrorArgumentRequired<T>(string valueName)
        {
            SetError($"Argument {valueName ?? "argument"} is not specified.");
            return default;
        }

        public void SetError(string error) => _error = error;

        public T GetDefault<T>(Func<T> valueDefault) => valueDefault != null ? valueDefault() : default;

        public void Finish()
        {
            if (_error != null)
                return;

            var index = _tokens.GetNextIndex();
            if (index >= 0)
            {
                _error = $"Option {_tokens[index]} is unknown.";
            }
        }
    }
}