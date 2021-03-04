using System.Collections.Generic;

namespace System.TinyCommandLine.Implementation
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
            if(_collectHelpText)
                return;

            _collectHelpText = true;
            _helpText = null;
            configure?.Invoke(new CommandBuilder(this));
            _collectHelpText = false;

            _commands ??= new List<CommandDesc>();
            _commands.Add(new CommandDesc(name, _helpText));
        }

        public void AddOption<T>(char shortName, string longName, OptionConfigurator<T> configure)
        {
            if(_collectHelpText)
                return;

            var state = new OptionState<T>();
            configure?.Invoke(new OptionBuilder<T>(state));

            if (state.IsHidden)
                return;

            bool isFlag = typeof(bool) == typeof(T);

            _options ??= new List<OptionDesc>();
            _options.Add(new OptionDesc(shortName, longName, state.ValueName, isFlag, state.HelpText, state.IsRequired));
        }

        public void Show<T>(string name, T helpBuilder) where T : IHelpBuilder
            => helpBuilder.Show(name, _helpText, _commands, _options);
    }
}