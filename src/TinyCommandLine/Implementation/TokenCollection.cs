using System.Collections;
using System.Collections.Generic;

namespace System.TinyCommandLine.Implementation
{
    class TokenCollection
    {
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

        public void EnumerateOptions(char shortName, string longName, bool isFlag, Action<int, int> action)
        {
            var (loShort, hiShort) = shortName != '\0' ? BinarySearchRange("-" + shortName) : default;
            var (loLong, hiLong) = longName != null ? BinarySearchRange("--" + longName) : default;

            while (loLong < hiLong || loShort < hiShort)
            {
                var indLong = loLong < hiLong ? _options[loLong].Index : int.MaxValue;
                var indShort = loShort < hiShort ? _options[loShort].Index : int.MaxValue;

                int index = indShort < indLong ? loShort++ : loLong++;
                var option = _options[index];
                if (_used[option.Index])
                    continue;

                MarkAsUsed(option.Index);

                var valOffset = option.Length + 1;
                var valIndex = option.Index;

                if (option.Str.Length == option.Length)
                {
                    valOffset = 0;
                    valIndex++;

                    if (!isFlag)
                    {
                        if (valIndex >= _tokens.Length)
                            throw ExceptionHelper.OptionHasNoValue(option.Str.Remove(option.Length));

                        MarkAsUsed(valIndex);
                    }
                }

                action(valIndex, valOffset);
            }
        }

        (int, int) BinarySearchRange(string name)
        {
            var lowerBound = ~_options.BinarySearch(new OptionInfo(name, name.Length, int.MinValue));

            var upperBound = ~_options.BinarySearch(lowerBound, _options.Count - lowerBound,
                new OptionInfo(name, name.Length, int.MaxValue), null);

            return (lowerBound, upperBound);
        }
    }
}