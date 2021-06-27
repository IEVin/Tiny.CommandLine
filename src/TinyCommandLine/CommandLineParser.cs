using System.Collections.Generic;
using System.TinyCommandLine.Implementation;

namespace System.TinyCommandLine
{
    public static class CommandLineParser
    {
        public static void Run(string name, string[] args, CommandConfigurator configure)
        {
            var tokens = TokenCollection.Tokenize(args);
            var state = new State();

            var commands = new List<string>(4);

            while (true)
            {
                var builder = new CommandBuilder(tokens, state);
                configure(builder);

                if (!state.IsHelpChecked)
                {
                    state.IsFinished = false;
                    builder.Option('h', "help", out state.IsHelpRequired);
                }

                if (state.IsHelpRequired)
                {
                    var help = new HelpCollector();
                    configure(new CommandBuilder(help));

                    // TODO: Add feature to change help builder from config
                    help.Show(name, commands, new DefaultHelpBuilder(Console.Out, 30));
                    break;
                }

                if(state.ErrReason != null)
                    break;

                if (state.SubCommandHandler != null)
                {
                    configure = state.SubCommandHandler;
                    commands.Add(state.SubCommandName);

                    state.SubCommandHandler = null;
                    state.SubCommandName = null;
                    state.IsFinished = false;
                    state.IsHelpChecked = false;
                    continue;
                }

                var index = tokens.GetNextIndex();
                if (index >= 0)
                {
                    state.ErrReason = $"Option {tokens[index]} is unknown.";
                }

                break;
            }

            if (state.ErrReason != null && !state.IsHelpRequired)
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