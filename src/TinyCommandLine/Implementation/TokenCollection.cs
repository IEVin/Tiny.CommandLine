using System.Collections;
using System.Collections.Generic;

namespace System.TinyCommandLine.Implementation
{
    class TokenCollection
    {
        struct OptionInfo
        {
            public int Index;
            public int ValueIndex;
            public bool IsLong;
        }

        readonly List<string> _tokens;
        readonly List<OptionInfo> _options;
        readonly BitArray _used;
        int _nextItemIndex;

        TokenCollection(List<string> tokens, List<OptionInfo> options)
        {
            _tokens = tokens;
            _options = options;
            _used = new BitArray(tokens.Count);
        }

        public static TokenCollection Tokenize(string[] args)
        {
            var tokens = new List<string>(args.Length * 2);
            var options = new List<OptionInfo>(args.Length);

            foreach (var q in args)
            {
                if (q.Length > 1 && q[0] == '-')
                {
                    var option = new OptionInfo
                    {
                        Index = tokens.Count,
                        IsLong = q.Length > 2 && q[1] == '-',
                        ValueIndex = q.IndexOf('=') + 1
                    };

                    options.Add(option);
                }

                tokens.Add(q);
            }

            options.Sort((a, b) => string.CompareOrdinal(tokens[a.Index], tokens[b.Index]));

            return new TokenCollection(tokens, options);
        }

        public int RemainingItemsCount => _tokens.Count - _nextItemIndex; // TODO: Add more accurate prediction

        public string this[int index] => _tokens[index];

        public void MarkAsUsed(int index) => _used[index] = true;

        public int GetNextIndex()
        {
            for (; _nextItemIndex < _used.Count; _nextItemIndex++)
            {
                if (_used[_nextItemIndex])
                    continue;

                return _nextItemIndex;
            }

            return -1;
        }

        public List<T> GetValues<T>(char shortName, string longName)
        {
            var result = new List<T>();

            for (var i = _nextItemIndex; i < _used.Count;)
            {
                var optionIndex = FindOptionIndex(shortName, longName, i);
                if (optionIndex < 0)
                    break;

                var info = _options[optionIndex];

                var index = info.ValueIndex == 0 ? info.Index + 1 : info.Index;
                i = index + 1;

                MarkAsUsed(info.Index);

                if (typeof(T) == typeof(bool) && info.ValueIndex == 0)
                {
                    result.Add(Converter<T>.Cast(true));
                    continue;
                }

                if (index >= _tokens.Count)
                    throw new InvalidSyntaxException($"The option --{longName} must have a value");

                MarkAsUsed(index);
                var rawValue = _tokens[index].Substring(info.ValueIndex);
                result.Add(Converter<T>.Parse(rawValue, longName));
            }

            return result;
        }

        int FindOptionIndex(char shortName, string longName, int startIndex)
        {
            var tokenInd = int.MaxValue;
            var optionInd = -1;

            // TODO: Change it to binary search
            if (shortName != '\0')
            {
                for (var i = 0; i < _options.Count; i++)
                {
                    var q = _options[i];
                    if (!q.IsLong && startIndex <= q.Index && _tokens[q.Index][1] == shortName)
                    {
                        if (q.Index < tokenInd)
                        {
                            tokenInd = q.Index;
                            optionInd = i;
                        }
                    }
                }
            }

            if (longName != null)
            {
                for (var i = 0; i < _options.Count; i++)
                {
                    var q = _options[i];

                    var len = q.ValueIndex == 0
                        ? _tokens[q.Index].Length - 2
                        : q.ValueIndex - 3;

                    if (q.IsLong && startIndex <= q.Index && len == longName.Length &&
                        string.CompareOrdinal(_tokens[q.Index], 2, longName, 0, longName.Length) == 0)
                    {
                        if (q.Index < tokenInd)
                        {
                            tokenInd = q.Index;
                            optionInd = i;
                        }
                    }
                }
            }

            return optionInd;
        }

    }
}