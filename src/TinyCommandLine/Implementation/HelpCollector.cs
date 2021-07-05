using System.Collections.Generic;

namespace Tiny.CommandLine.Implementation
{
    class HelpCollector
    {
        List<CommandDesc> _commands;
        List<OptionDesc> _options;
        string _helpText;
        bool _collectHelpText;

        public void HelpText(string text) => _helpText = text;

        public void AddCommand(string name, CommandConfigurator configure)
        {
            if (_collectHelpText)
                return;

            _collectHelpText = true;
            var currentHelpText = _helpText;
            _helpText = null;

            configure?.Invoke(new CommandBuilder(new Parser(this)));

            _commands ??= new List<CommandDesc>();
            _commands.Add(new CommandDesc(name, _helpText));

            _helpText = currentHelpText;
            _collectHelpText = false;
        }

        public void AddOption<T>(char shortName, string longName, OptionConfigurator<T> configure, bool isList)
        {
            if (_collectHelpText)
                return;

            var state = new OptionState<T>();
            configure?.Invoke(new OptionBuilder<T>(state));

            if (state.IsHidden)
                return;

            bool isFlag = typeof(T) == typeof(bool);

            _options ??= new List<OptionDesc>();
            _options.Add(new OptionDesc(shortName, longName, state.ValueName, state.HelpText, state.IsRequired, isFlag, isList));
        }

        public void Show<T>(string name, List<string> commandParts, T helpBuilder) where T : IHelpBuilder
            => helpBuilder.Show(name, _helpText, commandParts, _commands, _options);
    }
}