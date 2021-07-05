using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Tiny.CommandLine.Implementation;

namespace Tiny.CommandLine
{
    public static class Extensions
    {
        const char NoShortName = Parser.NoShortName;
        const string NoLongName = Parser.NoLongName;

        public static CommandBuilder Variable<T>(this CommandBuilder builder, out T variable, T defaultValue = default)
        {
            variable = defaultValue;
            return builder;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static OptionConfigurator<T> SetHelpText<T>(string text) => x => x.HelpText(text);

        #region Option(out T value)

        public static CommandBuilder Option<T>(this CommandBuilder builder, char shortName, string longName, out T value, string helpText)
            => builder.Option(shortName, longName, out value, SetHelpText<T>(helpText));

        public static CommandBuilder Option<T>(this CommandBuilder builder, char shortName, out T value, OptionConfigurator<T> configure = null)
            => builder.Option(shortName, NoLongName, out value, configure);

        public static CommandBuilder Option<T>(this CommandBuilder builder, char shortName, out T value, string helpText)
            => builder.Option(shortName, NoLongName, out value, SetHelpText<T>(helpText));

        public static CommandBuilder Option<T>(this CommandBuilder builder, string longName, out T value, OptionConfigurator<T> configure = null)
            => builder.Option(NoShortName, longName, out value, configure);

        public static CommandBuilder Option<T>(this CommandBuilder builder, string longName, out T value, string helpText)
            => builder.Option(NoShortName, longName, out value, SetHelpText<T>(helpText));


        public static CommandBuilder Argument<T>(this CommandBuilder builder, out T value, string helpText)
            => builder.Argument(out value, SetHelpText<T>(helpText));

        #endregion

        #region OptionList(out IReadOnlyList<T> value)

        public static CommandBuilder OptionList<T>(this CommandBuilder builder, char shortName, string longName, out IReadOnlyList<T> value, string helpText)
            => builder.OptionList(shortName, longName, out value, SetHelpText<IReadOnlyList<T>>(helpText));

        public static CommandBuilder OptionList<T>(this CommandBuilder builder, char shortName, out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
            => builder.OptionList(shortName, NoLongName, out value, configure);

        public static CommandBuilder OptionList<T>(this CommandBuilder builder, char shortName, out IReadOnlyList<T> value, string helpText)
            => builder.OptionList(shortName, NoLongName, out value, SetHelpText<IReadOnlyList<T>>(helpText));

        public static CommandBuilder OptionList<T>(this CommandBuilder builder, string longName, out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
            => builder.OptionList(NoShortName, longName, out value, configure);

        public static CommandBuilder OptionList<T>(this CommandBuilder builder, string longName, out IReadOnlyList<T> value, string helpText)
            => builder.OptionList(NoShortName, longName, out value, SetHelpText<IReadOnlyList<T>>(helpText));

        public static CommandBuilder ArgumentList<T>(this CommandBuilder builder, out IReadOnlyList<T> value, OptionConfigurator<IReadOnlyList<T>> configure = null)
            => builder.ArgumentList(out value, configure);

        public static CommandBuilder ArgumentList<T>(this CommandBuilder builder, out IReadOnlyList<T> value, string helpText)
            => builder.ArgumentList(out value, SetHelpText<IReadOnlyList<T>>(helpText));

        #endregion
    }
}