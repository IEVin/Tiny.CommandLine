namespace System.TinyCommandLine.Implementation
{
    class OptionState<T>
    {
        public string ValueName;
        public string HelpText;
        public T DefaultValue;
        public bool IsRequired;
        public bool IsHidden;
    }

    public readonly ref struct OptionBuilder<T>
    {
        readonly OptionState<T> _state;
        internal OptionBuilder(OptionState<T> state) => _state = state;

        public OptionBuilder<T> ValueName(string name) => Set(out _state.ValueName, name);
        public OptionBuilder<T> HelpText(string text) => Set(out _state.HelpText, text);
        public OptionBuilder<T> Hidden() => Set(out _state.IsHidden, true);
        public OptionBuilder<T> Default(T value) => Set(out _state.DefaultValue, value);
        public OptionBuilder<T> Required() => Set(out _state.IsRequired, true);

        OptionBuilder<T> Set<TValue>(out TValue field, TValue value)
        {
            field = value;
            return this;
        }
    }
}