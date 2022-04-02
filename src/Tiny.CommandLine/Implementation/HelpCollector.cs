using System.Collections.Generic;

namespace Tiny.CommandLine.Implementation
{
    class HelpCollector
    {
        readonly LinkedList<Command> _commands = new LinkedList<Command>();
        readonly LinkedList<Option> _options = new LinkedList<Option>();
        readonly LinkedList<string> _commandParts = new LinkedList<string>();
        readonly string _name;
        readonly string _helpText;

        public HelpCollector(string name, string helpText)
        {
            _name = name;
            _helpText = helpText;
        }

        public void AddCommand(string name, string helpText)
        {
            _commands.AddLast(new Command(name, helpText));
        }

        public void AddOption<T>(char alias, string name, string helpText, string valueName, bool required, bool list)
        {
            _options.AddLast(new Option(alias, name, helpText, required, valueName, list, OptionParser.IsFlag<T>()));
        }

        public void EnterCommand(string name)
        {
            Clear();

            _commandParts.AddLast(name);
        }

        public void Clear()
        {
            _commands.Clear();
            _options.Clear();
        }

        public void Show<T>(T helpBuilder) where T : IHelpBuilder
            => helpBuilder.Show(_name, _helpText, _commandParts, _commands, _options);
    }
}