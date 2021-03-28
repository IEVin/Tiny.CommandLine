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

using System.TinyCommandLine.Implementation;

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
    public static class CommandLineParser
    {
        public static void Run(string name, string[] args, CommandConfigurator configure)
        {
            var tokens = TokenCollection.Tokenize(args);
            var state = new State();

            while (true)
            {
                var builder = new CommandBuilder(tokens, state);
                configure(builder);

                var index = tokens.GetNextIndex();
                if (index >= 0 && (tokens[index] == "-h" || tokens[index] == "--help"))
                {
                    var help = new HelpCollector();
                    configure(new CommandBuilder(help));

                    help.Show<IHelpBuilder>(name, null);
                    return;
                }

                if(state.ErrReason != null)
                    break;

                if (state.SubCommand != null)
                {
                    configure = state.SubCommand;
                    state.SubCommand = null;
                    state.IsFinished = false;
                    continue;
                }

                index = tokens.GetNextIndex();
                if (index >= 0)
                {
                    state.ErrReason = $"Option {tokens[index]} is unknown.";
                }

                break;
            }

            if (state.ErrReason != null)
            {
                ShowError(state.ErrReason);
                return;
            }

            state.Handler?.Invoke();
        }

        static void ShowError(string text)
        {
            Console.WriteLine(text);
            //throw new NotImplementedException();
        }
    }
}