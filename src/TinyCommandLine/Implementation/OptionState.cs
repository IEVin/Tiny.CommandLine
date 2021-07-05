namespace Tiny.CommandLine.Implementation
{
    class OptionState<T>
    {
        public string ValueName;
        public string HelpText;
        public T DefaultValue;
        public bool IsRequired;
        public bool IsHidden;
    }
}