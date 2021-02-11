namespace System.TinyCommandLine.Implementation
{
    class HelpGenerator
    {
        public void AddCommand(string name, CommandConfigurator configure) => throw new NotImplementedException();

        public void AddOption<T>(char shortName, string longName, OptionConfigurator<T> configure) => throw new NotImplementedException();

        public void AddArgument<T>(OptionConfigurator<T> configure) => throw new NotImplementedException();

        public void AddDesc(string text) => throw new NotImplementedException();

        public void Show() => throw new NotImplementedException();
    }
}