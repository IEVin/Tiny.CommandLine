using System;
using System.Collections.Generic;
using Tiny.CommandLine.Implementation;

namespace Tiny.CommandLine
{
    public static class ParserExtensions
    {
        // This action is used to override it only in tests
        static Action<int> ExitAction = Environment.Exit;

        public static void Run(this CommandLineParser parser)
        {
            var result = parser.GetResult();
            if (result)
                return;

            var exitCode = result == ParserResult.State.Error ? Constants.ErrorCode : 0;
            ExitAction(exitCode);
        }

        public static CommandLineParser Option<T>(this CommandLineParser parser, char alias, out T value, string helpText = null,
            Func<T> valueDefault = null, bool required = false, string valueName = null, bool hidden = false)
            => parser.Option(alias, Constants.NoName, out value, helpText, valueDefault, required, valueName, hidden);

        public static CommandLineParser Option<T>(this CommandLineParser parser, string name, out T value, string helpText = null,
            Func<T> valueDefault = null, bool required = false, string valueName = null, bool hidden = false)
            => parser.Option(Constants.NoAlias, name, out value, helpText, valueDefault, required, valueName, hidden);

        public static CommandLineParser OptionList<T>(this CommandLineParser parser, string name, out IReadOnlyList<T> value, string helpText = null,
            Func<IReadOnlyList<T>> valueDefault = null, bool required = false, string valueName = null, bool hidden = false)
            => parser.OptionList(Constants.NoAlias, name, out value, helpText, valueDefault, required, valueName, hidden);

        public static CommandLineParser OptionList<T>(this CommandLineParser parser, char alias, out IReadOnlyList<T> value, string helpText = null,
            Func<IReadOnlyList<T>> valueDefault = null, bool required = false, string valueName = null, bool hidden = false)
            => parser.OptionList(alias, Constants.NoName, out value, helpText, valueDefault, required, valueName, hidden);
    }
}