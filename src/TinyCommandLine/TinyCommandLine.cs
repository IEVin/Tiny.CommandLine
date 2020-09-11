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

using System.Runtime.CompilerServices;

// TODO:
// - usage for commands
// - commands appearance as options (like --help or --version)
// - default value for property specified as flag
// - argument value name
// - enum support
// - print syntax


namespace System.TinyCommandLine
{
    using Implementation;

    public class CommandLineParser
    {
        const string NoCommand = null;

        public delegate void CommandConfigurator(CommandBuilder builder);

        public delegate void CommandConfigurator<in T>(T options, CommandBuilder builder);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CommandLineParser Command(CommandConfigurator configure)
            => Command(NoCommand, configure);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CommandLineParser Command<T>(Func<T> ctor, CommandConfigurator<T> configure) where T : new()
            => Command(b => configure(ctor(), b));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CommandLineParser Command<T>(string name, Func<T> ctor, CommandConfigurator<T> configure)
            => Command(name, b => configure(ctor(), b));


        public CommandLineParser Command(string name, CommandConfigurator configure) => throw new NotImplementedException(name);
        public CommandLineParser ErrorHandler(Func<string> handler) => throw new NotImplementedException();
        public void Run() => throw new NotImplementedException();
    }

    namespace Implementation
    {
        public ref struct CommandBuilder
        {
            const char NoShortName = '\0';
            const string NoLongName = null;

            public delegate void OptionConfigurator<T>(OptionBuilder<T> builder);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CommandBuilder CreateOptions<T>(T optionsDefaultObject, out T options)
            {
                options = optionsDefaultObject;
                return this;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CommandBuilder Option<T>(string longName, out T value, string helpText)
                => Option(NoShortName, longName, out value, b => b.HelpText(helpText));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CommandBuilder Option<T>(string longName, out T value, OptionConfigurator<T> configure = null)
                => Option(NoShortName, longName, out value, configure);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CommandBuilder Option<T>(char shortName, out T value, string helpText)
                => Option(shortName, NoLongName, out value, b => b.HelpText(helpText));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CommandBuilder Option<T>(char shortName, out T value, OptionConfigurator<T> configure)
                => Option(shortName, NoLongName, out value, configure);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CommandBuilder Option<T>(out T value, string helpText)
                => Option(NoShortName, NoLongName, out value, b => b.HelpText(helpText));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CommandBuilder Option<T>(out T value, OptionConfigurator<T> configure = null)
                => Option(NoShortName, NoLongName, out value, configure);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CommandBuilder Option<T>(char shortName, string longName, out T value, string helpText)
                => Option(shortName, longName, out value, b => b.HelpText(helpText));


            public CommandBuilder Option<T>(char shortName, string longName, out T value, OptionConfigurator<T> configure = null)
                => throw new NotImplementedException();

            public CommandBuilder Check(Func<bool> predicate, string message) => throw new NotImplementedException();

            public CommandBuilder HelpText(string message) => throw new NotImplementedException();

            public void Invoke(Action handler) => throw new NotImplementedException();
        }

        public ref struct OptionBuilder<T>
        {
            public OptionBuilder<T> HelpText(string str) => throw new NotImplementedException();
            public OptionBuilder<T> Hidden() => throw new NotImplementedException();
            public OptionBuilder<T> Default(T value) => throw new NotImplementedException();
            public OptionBuilder<T> Required() => throw new NotImplementedException();
        }
    }

    namespace Extensions
    {
        public static class CommandLineParserExtensions
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static CommandLineParser Command<T>(this CommandLineParser parser, CommandLineParser.CommandConfigurator<T> configure) where T : new()
                => parser.Command(b => configure(new T(), b));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static CommandLineParser Command<T>(this CommandLineParser parser, string name, CommandLineParser.CommandConfigurator<T> configure) where T : new()
                => parser.Command(name, b => configure(new T(), b));
        }

        public static class CommandBuilderExtensions
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static CommandBuilder CreateOptions<T>(this CommandBuilder builder, out T options)
                where T : new()
                => builder.CreateOptions(new T(), out options);
        }
    }
}