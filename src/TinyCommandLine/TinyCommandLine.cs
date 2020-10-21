////////////////////////////////////////////////////////////////////////////
// Copyright 2020 Ivan Vinogradov
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

// TODO:
// - usage for commands
// - commands appearance as options (like --help or --version)
// - default value for property specified as flag
// - argument value name
// - enum support
// - print syntax
// - custom error handler
// - message localization


namespace System.TinyCommandLine
{
    using Implementation;

    public static class CommandLineParser
    {
        public static void Run(string[] args, CommandConfigurator configure)
        {
            Action handler = null;

            try
            {
                var tokens = TokenCollection.Tokenize(args);
                var state = new State();

                while (true)
                {
                    state.Count = tokens.Count - state.StartIndex;

                    configure(new CommandBuilder(tokens, state));

                    if (state.IsHelpRequired)
                    {
                        var helpGen = new HelpGenerator();
                        configure(new CommandBuilder(helpGen));

                        handler = helpGen.Show;
                        break;
                    }

                    if (state.Command == null)
                    {
                        handler = state.Handler;
                        break;
                    }

                    configure = state.Command;
                    state.Command = null;
                    state.StartIndex += state.Count + 1;
                } 
            }
            catch (InvalidSyntaxException ex)
            {
                ShowError(ex.Message);
            }
            catch (CheckFailedException ex)
            {
                ShowError(ex.Message);
            }

            handler?.Invoke();
        }

        public static void ShowError(string text)
        {
            Console.WriteLine(text);
            //throw new NotImplementedException();
        }
    }

    namespace Implementation
    {
        public delegate void CommandConfigurator(CommandBuilder builder);

        public delegate void OptionConfigurator<T>(OptionBuilder<T> builder);

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
                if (_helpGen != null)
                {
                    _helpGen.AddCommand(name, configure);
                    return this;
                }

                var index = _tokens.GetIndex(name, _state.StartIndex, _state.Count);
                if (index >= 0)
                {
                    _state.Count = index - _state.StartIndex;
                    _state.Command = configure;
                }

                // TODO: Add check for duplicate commands
                return this;
            }


            public CommandBuilder Option<T>(char shortName, string longName, out T value, OptionConfigurator<T> configure = null)
                => OptionsInternal<T, T>(shortName, longName, out value, configure, x => x[x.Count - 1]);

            public CommandBuilder OptionList<T>(char shortName, string longName, out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
                => OptionsInternal<IReadOnlyList<T>, T>(shortName, longName, out value, configure, x => x);


            CommandBuilder OptionsInternal<T, TItem>(char shortName, string longName, out T value, OptionConfigurator<T> configure, Func<List<TItem>, T> func)
            {
                if (_helpGen != null)
                {
                    _helpGen.AddOption(shortName, longName, configure);
                    value = default;
                    return this;
                }

                var result = _tokens.GetValues<TItem>(shortName, longName, _state.StartIndex, _state.Count);
                if (result.Count != 0)
                {
                    value = func(result);
                    return this;
                }

                var optionState = new OptionState<T>();
                configure?.Invoke(new OptionBuilder<T>(optionState));

                if (optionState.IsRequired)
                    throw ExceptionHelper.OptionNotSpecified(longName);

                value = optionState.DefaultValue;
                return this;
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

                if (_state.Command == null)
                {
                    _state.Handler = handler;
                }
            }
        }

        public readonly ref struct OptionBuilder<T>
        {
            readonly OptionState<T> _state;

            internal OptionBuilder(OptionState<T> state) => _state = state;

            public OptionBuilder<T> HelpText(string text)
            {
                _state.Text = text;
                return this;
            }

            public OptionBuilder<T> Hidden()
            {
                _state.IsHidden = true;
                return this;
            }

            public OptionBuilder<T> Default(T value)
            {
                _state.DefaultValue = value;
                return this;
            }

            public OptionBuilder<T> Required()
            {
                _state.IsRequired = true;
                return this;
            }
        }

        public class InvalidSyntaxException : Exception
        {
            public InvalidSyntaxException(string message) : base(message) { }
        }

        public class CheckFailedException : Exception
        {
            public CheckFailedException(string message) : base(message) { }
        }

        static class Converter<T>
        {
            // This method will be optimized with jit and should do zero cpu instruction
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T Cast<TIn>(TIn value) => (T) (object) value;

            public static T Parse(string str, string optionName)
            {
                if (typeof(T) == typeof(string))
                    return Cast(str);

                if (typeof(T) == typeof(char))
                {
                    return str.Length == 1
                        ? Cast(str[0])
                        : throw ExceptionHelper.InvalidOptionType(optionName, "char");
                }

                if (typeof(T) == typeof(bool))
                {
                    if (str == "True" || str == "true" || str == "1")
                        return Cast(true);
                    if (str == "False" || str == "false" || str == "0")
                        return Cast(false);

                    throw ExceptionHelper.InvalidOptionType(optionName, "bool");
                }

                if (typeof(T) == typeof(int))
                {
                    return int.TryParse(str, out var result)
                        ? Cast(result)
                        : throw ExceptionHelper.InvalidOptionType(optionName, "int");
                }

                if (typeof(T) == typeof(double))
                {
                    return double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
                        ? Cast(result)
                        : throw ExceptionHelper.InvalidOptionType(optionName, "double");
                }

                if (typeof(T) == typeof(DateTime))
                {
                    return DateTime.TryParse(str, out var result)
                        ? Cast(result)
                        : throw ExceptionHelper.InvalidOptionType(optionName, nameof(DateTime));
                }

                throw new NotSupportedException($"The type of option '{optionName}' is not supported");
            }
        }

        class State
        {
            public CommandConfigurator Command;
            public Action Handler;
            public int StartIndex;
            public int Count;
            public bool IsHelpRequired;
        }

        class OptionState<T>
        {
            public string Text;
            public T DefaultValue;
            public bool IsRequired;
            public bool IsHidden;
        }

        class HelpGenerator
        {
            public void AddCommand(string name, CommandConfigurator configure) => throw new NotImplementedException();

            public void AddOption<T>(char shortName, string longName, OptionConfigurator<T> configure) => throw new NotImplementedException();

            public void AddDesc(string text) => throw new NotImplementedException();

            public void Show() => throw new NotImplementedException();
        }

        static class ExceptionHelper
        {
            public static Exception OptionNotSpecified(string name) => new InvalidSyntaxException($"Option --{name} not specified.");
            public static Exception InvalidOptionType(string name, string type) => new InvalidSyntaxException($"Option --{name} must be a {type}.");
        }

        class TokenCollection
        {
            public struct OptionInfo
            {
                public int Index;
                public int ValueIndex;
                public bool IsLong;
            }

            readonly List<string> _tokens;
            readonly List<OptionInfo> _options;

            public int Count => _tokens.Count;

            TokenCollection(List<string> tokens, List<OptionInfo> options) => (_tokens, _options) = (tokens, options);

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

                options.Sort((a, b) => Comparer<string>.Default.Compare(tokens[a.Index], tokens[b.Index]));

                return new TokenCollection(tokens, options);
            }
            
            public int GetIndex(string name, int index, int count) => _tokens.IndexOf(name, index, count);

            public List<T> GetValues<T>(char shortName, string longName, int startIndex, int count)
            {
                var result = new List<T>();

                int endIndex = startIndex + count;
                for (var propIndex = startIndex; propIndex < endIndex;)
                {
                    var optionIndex = FindOptionIndex(shortName, longName, propIndex);
                    if (optionIndex < 0)
                        break;

                    var info = _options[optionIndex];

                    var index = info.ValueIndex == 0 ? info.Index + 1 : info.Index;
                    propIndex = index + 1;

                    if (typeof(T) == typeof(bool) && info.ValueIndex == 0)
                    {
                        result.Add(Converter<T>.Cast(true));
                        continue;
                    }

                    if (index >= _tokens.Count)
                        throw new InvalidSyntaxException($"The option --{longName} must have a value");

                    var rawValue = _tokens[index].Substring(info.ValueIndex);
                    result.Add(Converter<T>.Parse(rawValue, longName));
                }

                return result;
            }

            int FindOptionIndex(char shortName, string longName, int index)
            {
                var tokenInd = int.MaxValue;
                var optionInd = -1;

                // TODO: Change it to binary search
                for (var i = 0; i < _options.Count; i++)
                {
                    var q = _options[i];
                    if (!q.IsLong && q.Index >= index && _tokens[q.Index][1] == shortName)
                    {
                        if (q.Index < tokenInd)
                        {
                            tokenInd = q.Index;
                            optionInd = i;
                        }
                    }
                }

                for (var i = 0; i < _options.Count; i++)
                {
                    var q = _options[i];

                    var len = q.ValueIndex == 0
                        ? _tokens[q.Index].Length - 2
                        : q.ValueIndex - 3;

                    if (q.IsLong && q.Index >= index && len == longName.Length &&
                        string.Compare(_tokens[q.Index], 2, longName, 0, longName.Length) == 0)
                    {
                        if (q.Index < tokenInd)
                        {
                            tokenInd = q.Index;
                            optionInd = i;
                        }
                    }
                }

                return optionInd;
            }
        }
    }


    public static class Extensions
    {
        const string NoLongName = null;
        const char NoShortName = '\0';

        public static CommandBuilder Variable<T>(this CommandBuilder builder, out T variable, T defaultValue = default)
        {
            variable = defaultValue;
            return builder;
        }

        #region Option(out T value)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder Option<T>(this CommandBuilder builder, string longName, out T value, string helpText)
            => builder.Option(NoShortName, longName, out value, b => b.HelpText(helpText));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder Option<T>(this CommandBuilder builder, string longName, out T value, OptionConfigurator<T> configure = null)
            => builder.Option(NoShortName, longName, out value, configure);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder Option<T>(this CommandBuilder builder, char shortName, out T value, string helpText)
            => builder.Option(shortName, NoLongName, out value, b => b.HelpText(helpText));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder Option<T>(this CommandBuilder builder, char shortName, out T value, OptionConfigurator<T> configure = null)
            => builder.Option(shortName, NoLongName, out value, configure);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder Option<T>(this CommandBuilder builder, out T value, string helpText)
            => builder.Option(NoShortName, NoLongName, out value, b => b.HelpText(helpText));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder Option<T>(this CommandBuilder builder, out T value, OptionConfigurator<T> configure = null)
            => builder.Option(NoShortName, NoLongName, out value, configure);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder Option<T>(this CommandBuilder builder, char shortName, string longName, out T value, string helpText)
            => builder.Option(shortName, longName, out value, b => b.HelpText(helpText));

        #endregion

        #region OptionList(out IReadOnlyList<T> value)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder OptionList<T>(this CommandBuilder builder, string longName, out IReadOnlyList<T> value, string helpText)
            => builder.OptionList(NoShortName, longName, out value, b => b.HelpText(helpText));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder OptionList<T>(this CommandBuilder builder, string longName, out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
            => builder.OptionList(NoShortName, longName, out value, configure);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder OptionList<T>(this CommandBuilder builder, char shortName, out IReadOnlyList<T> value, string helpText)
            => builder.OptionList(shortName, NoLongName, out value, b => b.HelpText(helpText));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder OptionList<T>(this CommandBuilder builder, char shortName, out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
            => builder.OptionList(shortName, NoLongName, out value, configure);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder OptionList<T>(this CommandBuilder builder, out IReadOnlyList<T> value, string helpText)
            => builder.OptionList(NoShortName, NoLongName, out value, b => b.HelpText(helpText));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder OptionList<T>(this CommandBuilder builder, out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
            => builder.OptionList(NoShortName, NoLongName, out value, configure);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CommandBuilder OptionList<T>(this CommandBuilder builder, char shortName, string longName, out IReadOnlyList<T> value, string helpText)
            => builder.OptionList(shortName, longName, out value, b => b.HelpText(helpText));

        #endregion
    }
}