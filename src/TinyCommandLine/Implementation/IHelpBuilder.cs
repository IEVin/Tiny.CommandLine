using System.Collections.Generic;

namespace System.TinyCommandLine.Implementation
{
    public interface IHelpBuilder
    {
        void Show(string name, string helpText, List<CommandDesc> commands, List<OptionDesc> options);
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

        public OptionDesc(char shortName, string longName, string valueName, bool isFlag, string helpText, bool isRequired)
        {
            ShortName = shortName;
            LongName = longName;
            ValueName = valueName;
            HelpText = helpText;
            IsFlag = isFlag;
            IsRequired = isRequired;
            IsArgument = longName == null && shortName == '\0';
        }
    }
}