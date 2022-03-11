using System.Collections.Generic;

namespace Tiny.CommandLine.Implementation
{
    public readonly struct Command
    {
        public readonly string Name;
        public readonly string HelpText;

        public Command(string name, string helpText)
        {
            Name = name;
            HelpText = helpText;
        }
    }

    public readonly struct Option
    {
        public readonly string ValueName;
        public readonly string HelpText;
        public readonly string Name;
        public readonly char Alias;
        public readonly bool IsRequired;
        public readonly bool IsArgument;
        public readonly bool IsFlag;
        public readonly bool IsList;

        public Option(char alias, string name, string helpText, bool required, string valueName, bool list, bool flag)
        {
            Alias = alias;
            Name = name;
            ValueName = valueName;
            HelpText = helpText;
            IsRequired = required;
            IsList = list;
            IsFlag = flag;
            IsArgument = name == Constants.NoName && alias == Constants.NoAlias;
        }
    }

    public interface IHelpBuilder
    {
        void Show(string name, string helpText, IReadOnlyCollection<string> commandParts, IReadOnlyCollection<Command> commands, IReadOnlyCollection<Option> options);
    }
}