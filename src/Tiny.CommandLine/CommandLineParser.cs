using System;
using System.Collections.Generic;
using Tiny.CommandLine.Implementation;

namespace Tiny.CommandLine
{
    public class CommandLineParser
    {
        enum State : byte
        {
            Active,
            Interrupted,
            Finished,
        }

        OptionParser _parser;
        HelpCollector _helpCollector;

        State _state;
        ParserResult.State _result;

        bool _helpChecked;

        public CommandLineParser(string[] args, string name, string helpText = null)
        {
            _parser = new OptionParser(args);
            _helpCollector = new HelpCollector(name, helpText);
        }


        public CommandLineParser Command(string name, string helpText, Action<CommandLineParser> builder, bool hidden = false)
        {
            if (_state != State.Active)
                return this;

            if (!hidden)
                _helpCollector.AddCommand(name, helpText);

            if (_parser.HasError)
                return this;

            bool isMatch = _parser.Command(name);
            if (!isMatch)
                return this;

            _helpCollector.EnterCommand(name);
            builder(this);

            _state = _state == State.Active ? State.Interrupted : _state;
            _result = _result == ParserResult.State.Success ? ParserResult.State.Handled : _result;

            return this;
        }

        public CommandLineParser Option<T>(char alias, string name, out T value, string helpText = null,
            Func<T> valueDefault = null, bool required = false, string valueName = null, bool hidden = false)
        {
            if (!TryCollectHelp<T>(alias, name, helpText, required, valueName, hidden, false))
            {
                value = default;
                return this;
            }

            value = _parser.Option(alias, name, valueDefault, required);
            return this;
        }

        public CommandLineParser OptionList<T>(char alias, string name, out IReadOnlyList<T> value, string helpText = null,
            Func<IReadOnlyList<T>> valueDefault = null, bool required = false, string valueName = null, bool hidden = false)
        {
            if (!TryCollectHelp<T>(alias, name, helpText, required, valueName, hidden, true))
            {
                value = default;
                return this;
            }

            value = _parser.OptionList(alias, name, valueDefault, required);
            return this;
        }

        public CommandLineParser Argument<T>(out T value, string helpText = null,
            Func<T> valueDefault = null, bool required = false, string valueName = null, bool hidden = false)
        {
            if (!TryCollectHelp<T>(Constants.NoAlias, Constants.NoName, helpText, required, valueName, hidden, false))
            {
                value = default;
                return this;
            }

            CheckHelp();

            value = _parser.Argument(valueDefault, required, valueName);
            return this;
        }

        public CommandLineParser ArgumentList<T>(out IReadOnlyList<T> value, string helpText = null,
            Func<IReadOnlyList<T>> valueDefault = null, bool required = false, string valueName = null, bool hidden = false)
        {
            if (!TryCollectHelp<T>(Constants.NoAlias, Constants.NoName, helpText, required, valueName, hidden, true))
            {
                value = default;
                return this;
            }

            CheckHelp();

            value = _parser.ArgumentList(valueDefault, required, valueName);
            return this;
        }


        public CommandLineParser Check(Func<bool> predicate, string message)
        {
            if (_state != State.Active || _parser.HasError)
                return this;

            if (!predicate())
            {
                _parser.SetError(message);
                _state = State.Interrupted;
            }

            return this;
        }

        public ParserResult GetResult()
        {
            if (_state == State.Finished)
                return new ParserResult(_result);

            Finish();

            // Manually cleanup all internal fields to avoid holding them during command handlers execution
            _parser = null;
            _helpCollector = null;

            return new ParserResult(_result);
        }


        void Finish()
        {
            _state = State.Finished;

            CheckHelp();
            if (_result == ParserResult.State.HelpRequired)
            {
                ShowHelp();
                return;
            }

            _parser.Finish();

            var error = _parser.ErrorReason;
            if (error != null)
            {
                _result = ParserResult.State.Error;
                ShowError(error);
                ShowHelp();
            }
        }

        bool TryCollectHelp<T>(char alias, string name, string helpText, bool required, string valueName, bool hidden, bool list)
        {
            if (_state != State.Active)
                return false;

            if (!hidden)
                _helpCollector.AddOption<T>(alias, name, helpText, valueName, required, list);

            return !_parser.HasError;
        }

        void CheckHelp()
        {
            if (_helpChecked)
                return;

            _helpChecked = true;

            var isHelpRequired = _parser.Option<bool>('h', "help", null, false);
            _result = isHelpRequired ? ParserResult.State.HelpRequired : _result;
        }

        void ShowHelp()
        {
            var output = _result == ParserResult.State.Error ? Console.Error : Console.Out;

            // TODO: Add feature to change help builder from config
            var helpBuilder = new DefaultHelpBuilder(output, 20, 40);
            _helpCollector.Show(helpBuilder);
        }

        static void ShowError(string text)
        {
            Console.Error.Write("Error: ");
            Console.Error.WriteLine(text);
            Console.Error.WriteLine();
        }
    }
}