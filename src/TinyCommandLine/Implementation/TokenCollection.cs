using System;
using System.Collections;
using System.Collections.Generic;

namespace Tiny.CommandLine.Implementation
{
    class TokenCollection
    {
        readonly string[] _tokens;
        readonly List<OptionInfo> _options;
        readonly BitArray _used;
        int _nextItemIndex;

        TokenCollection(string[] tokens, List<OptionInfo> options)
        {
            _tokens = tokens;
            _options = options;
            _used = new BitArray(tokens.Length);
        }

        public static TokenCollection Tokenize(string[] args)
        {
            var options = new List<OptionInfo>(args.Length);

            for (int i = 0; i < args.Length; i++)
            {
                var q = args[i];
                if (q.Length <= 1 || q[0] != '-')
                    continue;

                int ind = q.IndexOf('=');
                var option = new OptionInfo(q, ind < 0 ? q.Length : ind, i);

                options.Add(option);
            }

            options.Sort();

            return new TokenCollection(args, options);
        }

        public int Count => _tokens.Length;

        public int RemainingItemsCount => _tokens.Length - _nextItemIndex; // TODO: Add more accurate prediction

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

        public OptionsIterator IterateOptions(char shortName, string longName, bool isFlag)
            => new OptionsIterator(this, shortName, longName, isFlag);

        void BinarySearchOptionRange(string name, out int lowerBound, out int upperBound)
        {
            lowerBound = ~_options.BinarySearch(new OptionInfo(name, name.Length, int.MinValue));

            upperBound = ~_options.BinarySearch(lowerBound, _options.Count - lowerBound,
                new OptionInfo(name, name.Length, int.MaxValue), null);
        }

        public struct OptionsIterator
        {
            readonly TokenCollection _owner;
            readonly int _lastShort;
            readonly int _lastLong;
            readonly bool _isFlag;

            int _indShort;
            int _indLong;

            internal OptionsIterator(TokenCollection owner, char shortName, string longName, bool isFlag)
                : this()
            {
                _owner = owner;
                _isFlag = isFlag;

                if (shortName != Constants.NoAlias)
                    _owner.BinarySearchOptionRange("-" + shortName, out _indShort, out _lastShort);

                if (longName != Constants.NoName)
                    _owner.BinarySearchOptionRange("--" + longName, out _indLong, out _lastLong);
            }

            public bool TryMoveNext(out int index, out int length)
            {
                var options = _owner._options;

                while (_indLong < _lastLong || _indShort < _lastShort)
                {
                    var tokenIndLong = _indLong < _lastLong ? options[_indLong].Index : int.MaxValue;
                    var tokenIndShort = _indShort < _lastShort ? options[_indShort].Index : int.MaxValue;

                    int optionIndex = tokenIndShort < tokenIndLong ? _indShort++ : _indLong++;

                    var option = options[optionIndex];
                    if (_owner._used[option.Index])
                        continue;

                    _owner.MarkAsUsed(option.Index);

                    index = option.Index;
                    length = option.Length;

                    if (option.Str.Length == length && !_isFlag && index + 1 < _owner._tokens.Length)
                        _owner.MarkAsUsed(index + 1);

                    return true;
                }

                index = default;
                length = default;
                return false;
            }
        }

        readonly struct OptionInfo : IComparable<OptionInfo>
        {
            public readonly string Str;
            public readonly int Index;
            public readonly int Length;

            public OptionInfo(string str, int length, int index)
            {
                Str = str;
                Index = index;
                Length = length;
            }

            public int CompareTo(OptionInfo other)
            {
                int len = Math.Max(Length, other.Length);

                int order = string.CompareOrdinal(Str, 0, other.Str, 0, len);
                return order != 0 ? order : Index.CompareTo(other.Index);
            }
        }
    }
}