using System.Collections.Generic;

namespace System.TinyCommandLine.Implementation
{
    class HelpCollector
    {
        List<CommandDesc> _commands;
        List<OptionDesc> _options;
        string _helpText;

        public void HelpText(string text) => _helpText = text;

        public void AddCommand(string name, CommandConfigurator configure)
        {
            _commands ??= new List<CommandDesc>();
            _commands.Add(new CommandDesc(name, string.Empty)); // TODO: Add help text
        }

        public void AddOption<T>(char shortName, string longName, OptionConfigurator<T> configure)
        {
            var state = new OptionState<T>();
            configure?.Invoke(new OptionBuilder<T>(state));

            if (state.IsHidden)
                return;

            _options ??= new List<OptionDesc>();
            _options.Add(new OptionDesc(shortName, longName, state.ValueName, state.HelpText, state.IsRequired));
        }

        public void Show<T>(T helpBuilder) where T : IHelpBuilder
            => helpBuilder?.Show(_helpText, _commands, _options);
    }
}