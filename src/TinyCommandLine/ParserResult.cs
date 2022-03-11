namespace Tiny.CommandLine
{
    public readonly struct ParserResult
    {
        public enum State : byte
        {
            Success = 0,
            Handled,
            HelpRequired,
            Error,
        }

        public readonly State Result;

        internal ParserResult(State result) => Result = result;

        public static implicit operator bool(ParserResult value) => value.Result == State.Success;
        public static implicit operator State(ParserResult value) => value.Result;
    }
}