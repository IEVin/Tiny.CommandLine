using System.TinyCommandLine.Implementation;

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

                    // TODO: Add feature to change help builder from config
                    help.Show(name, new DefaultHelpBuilder(Console.Out, 30));
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