using System.Collections.Generic;

namespace Tiny.CommandLine.Implementation
{
    public interface IHelpBuilder
    {
        void Show(string name, string helpText, List<string> commandParts, List<CommandDesc> commands, List<OptionDesc> options);
    }

    public readonly struct CommandDesc
    {
        public readonly string Name;
        public readonly string HelpText;

        public CommandDesc(string name, string helpText)
        {
            Name = name;
            HelpText = helpText;
        }
    }

    public readonly struct OptionDesc
    {
        public readonly string ValueName;
        public readonly string HelpText;
        public readonly string LongName;
        public readonly char ShortName;
        public readonly bool IsRequired;
        public readonly bool IsArgument;
        public readonly bool IsFlag;
        public readonly bool IsList;

        public OptionDesc(char shortName, string longName, string valueName, string helpText, bool isRequired, bool isFlag, bool isList)
        {
            ShortName = shortName;
            LongName = longName;
            ValueName = valueName;
            HelpText = helpText;
            IsRequired = isRequired;
            IsFlag = isFlag;
            IsList = isList;
            IsArgument = longName == null && shortName == '\0';
        }
    }
}