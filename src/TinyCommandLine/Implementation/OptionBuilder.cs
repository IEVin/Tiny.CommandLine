namespace System.TinyCommandLine.Implementation
{
    class OptionState<T>
    {
        public string Text;
        public T DefaultValue;
        public bool IsRequired;
        public bool IsHidden;
    }

    public readonly ref struct OptionBuilder<T>
    {
        readonly OptionState<T> _state;

        internal OptionBuilder(OptionState<T> state) => _state = state;

        public OptionBuilder<T> HelpText(string text)
        {
            _state.Text = text;
            return this;
        }

        public OptionBuilder<T> Hidden()
        {
            _state.IsHidden = true;
            return this;
        }

        public OptionBuilder<T> Default(T value)
        {
            _state.DefaultValue = value;
            return this;
        }

        public OptionBuilder<T> Required()
        {
            _state.IsRequired = true;
            return this;
        }
    }
}