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

        public bool IsUsed(int index) => _used[index];
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

        public OptionsIterator IterateOptions(char alias, string name) => new OptionsIterator(this, alias, name);

        void BinarySearchOptionRange(string name, out int lowerBound, out int upperBound)
        {
            lowerBound = ~_options.BinarySearch(new OptionInfo(name, name.Length, int.MinValue));

            upperBound = ~_options.BinarySearch(lowerBound, _options.Count - lowerBound,
                new OptionInfo(name, name.Length, int.MaxValue), null);
        }

        public struct OptionsIterator
        {
            readonly TokenCollection _owner;
            readonly int _lastAlias;
            readonly int _lastName;

            int _indAlias;
            int _indName;

            internal OptionsIterator(TokenCollection owner, char alias, string name)
                : this()
            {
                _owner = owner;

                if (alias != Constants.NoAlias)
                    _owner.BinarySearchOptionRange("-" + alias, out _indAlias, out _lastAlias);

                if (name != Constants.NoName)
                    _owner.BinarySearchOptionRange("--" + name, out _indName, out _lastName);
            }

            public bool TryMoveNext(out int index, out int length)
            {
                var options = _owner._options;

                while (_indName < _lastName || _indAlias < _lastAlias)
                {
                    var tokenIndName = _indName < _lastName ? options[_indName].Index : int.MaxValue;
                    var tokenIndAlias = _indAlias < _lastAlias ? options[_indAlias].Index : int.MaxValue;

                    int optionIndex = tokenIndAlias < tokenIndName ? _indAlias++ : _indName++;

                    var option = options[optionIndex];
                    if (_owner._used[option.Index])
                        continue;

                    _owner.MarkAsUsed(option.Index);

                    index = option.Index;
                    length = option.Length;
                    return true;
                }

                index = default;
                length = default;
                return false;
            }
        }

        readonly struct OptionInfo : IComparable<OptionInfo>
        {
            public readonly int Index;
            public readonly int Length;
            readonly string _str;

            public OptionInfo(string str, int length, int index)
            {
                _str = str;
                Index = index;
                Length = length;
            }

            public int CompareTo(OptionInfo other)
            {
                int len = Math.Max(Length, other.Length);

                int order = string.CompareOrdinal(_str, 0, other._str, 0, len);
                return order != 0 ? order : Index.CompareTo(other.Index);
            }
        }
    }
}