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
            if (_state != State.Active)
            {
                value = default;
                return this;
            }

            if (!hidden)
                _helpCollector.AddOption<T>(alias, name, helpText, valueName, required, false);

            value = _parser.Option(alias, name, valueDefault, required);
            _state = _parser.HasError ? State.Interrupted : State.Active;
            return this;
        }

        public CommandLineParser OptionList<T>(char alias, string name, out IReadOnlyList<T> value, string helpText = null,
            Func<IReadOnlyList<T>> valueDefault = null, bool required = false, string valueName = null, bool hidden = false)
        {
            if (_state != State.Active)
            {
                value = default;
                return this;
            }

            if (!hidden)
                _helpCollector.AddOption<T>(alias, name, helpText, valueName, required, true);

            value = _parser.OptionList(alias, name, valueDefault, required);
            _state = _parser.HasError ? State.Interrupted : State.Active;
            return this;
        }

        public CommandLineParser Argument<T>(out T value, string helpText = null,
            Func<T> valueDefault = null, bool required = false, string valueName = null, bool hidden = false)
        {
            if (_state != State.Active)
            {
                value = default;
                return this;
            }

            if (!hidden)
                _helpCollector.AddOption<T>(Constants.NoAlias, Constants.NoName, helpText, valueName, required, false);

            CheckHelp();

            value = _parser.Argument(valueDefault, required, valueName);
            _state = _parser.HasError ? State.Interrupted : State.Active;
            return this;
        }

        public CommandLineParser ArgumentList<T>(out IReadOnlyList<T> value, string helpText = null,
            Func<IReadOnlyList<T>> valueDefault = null, bool required = false, string valueName = null, bool hidden = false)
        {
            if (_state != State.Active)
            {
                value = default;
                return this;
            }

            if (!hidden)
                _helpCollector.AddOption<T>(Constants.NoAlias, Constants.NoName, helpText, valueName, required, true);

            CheckHelp();

            value = _parser.ArgumentList(valueDefault, required, valueName);
            _state = _parser.HasError ? State.Interrupted : State.Active;
            return this;
        }


        public CommandLineParser Check(Func<bool> predicate, string message)
        {
            if (_state != State.Active)
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
            var helpBuilder = new DefaultHelpBuilder(output, 40);
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