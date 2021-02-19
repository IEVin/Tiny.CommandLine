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
        public static void Run(string[] args, CommandConfigurator configure)
        {
            Action handler = null;

            try
            {
                var tokens = TokenCollection.Tokenize(args);
                var state = new State();

                while (true)
                {
                    configure(new CommandBuilder(tokens, state));

                    if (state.IsHelpRequired)
                    {
                        var helpGen = new HelpGenerator();
                        configure(new CommandBuilder(helpGen));

                        handler = helpGen.Show;
                        break;
                    }

                    if (state.SubCommand == null)
                    {
                        handler = state.Handler;
                        break;
                    }

                    configure = state.SubCommand;
                    state.SubCommand = null;
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

        static void ShowError(string text)
        {
            Console.WriteLine(text);
            //throw new NotImplementedException();
        }
    }
}