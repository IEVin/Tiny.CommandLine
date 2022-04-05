using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tiny.CommandLine.Implementation
{
    readonly struct DefaultHelpBuilder : IHelpBuilder
    {
        readonly char[] _align;
        readonly TextWriter _writer;

        readonly int _outputWidth;
        readonly int _optionAlign;
        readonly int _commandAlign;

        public DefaultHelpBuilder(TextWriter writer, int commandAlign, int optionAlign)
        {
            var alignSize = Math.Max(optionAlign, commandAlign);

            _writer = writer;
            _outputWidth = Console.IsOutputRedirected ? int.MaxValue : Math.Max(alignSize, Console.WindowWidth);
            _optionAlign = Math.Min(optionAlign, _outputWidth - 5);
            _commandAlign = Math.Min(commandAlign, _outputWidth - 5);

            _align = new char[alignSize];
            for (int i = 0; i < _align.Length; i++)
                _align[i] = ' ';
        }

        public void Show(string name, string helpText, ICollection<string> commandParts, ICollection<Command> commands, ICollection<Option> options)
        {
            if (!string.IsNullOrEmpty(helpText))
            {
                _writer.WriteLine(helpText);
                _writer.WriteLine();
            }

            ShowSyntax(name, commandParts, commands, options);

            ShowCommands(commands);

            ShowOptions(options);
        }

        void ShowSyntax(string fileName, ICollection<string> commandParts, ICollection<Command> commands, ICollection<Option> options)
        {
            if (options.Count == 0 && commands.Count == 0)
                return;

            _writer.Write("Usage: ");
            _writer.Write(fileName);

            foreach (var part in commandParts)
            {
                _writer.Write(' ');
                _writer.Write(part);
            }

            int argumentNum = 0;
            foreach (var desc in options)
            {
                _writer.Write(' ');

                if (desc.IsArgument)
                    argumentNum++;

                var name = GetOptionName(desc, argumentNum, true);
                if (!desc.IsRequired)
                {
                    _writer.Write('[');
                    _writer.Write(name);
                    _writer.Write(']');
                }
                else if (desc.IsArgument)
                {
                    _writer.Write('<');
                    _writer.Write(name);
                    _writer.Write('>');
                }
                else
                {
                    _writer.Write(name);
                }
            }

            if (commands.Count > 0)
            {
                _writer.Write(" <command> [args]");
            }

            _writer.WriteLine();
            _writer.WriteLine();
        }

        void ShowCommands(ICollection<Command> commands)
        {
            if (commands.Count == 0)
                return;

            foreach (var desc in commands)
            {
                PrintValue(desc.Name, desc.HelpText, _commandAlign, 2);
            }

            Console.WriteLine();
        }

        void ShowOptions(ICollection<Option> options)
        {
            int argumentNum = 0;
            foreach (var desc in options)
            {
                if (desc.IsArgument)
                {
                    if (argumentNum == 0)
                        _writer.WriteLine();

                    argumentNum++;
                }

                string name = GetOptionName(desc, argumentNum, false);
                PrintValue(name, desc.HelpText, _optionAlign, 4);
            }
        }

        string GetOptionName(Option desc, int argumentNum, bool shortForm)
        {
            // format "   -a, --name <value>"
            var len = 12 + (desc.Name?.Length ?? 0) + (desc.ValueName?.Length ?? 0);
            var sb = new StringBuilder(len);

            // argument
            if (desc.IsArgument)
            {
                var argument = desc.ValueName ?? "argument" + argumentNum;
                sb.Append(argument);

                if (desc.IsList)
                    sb.Append("...");

                return sb.ToString();
            }

            // option
            if (desc.Alias != Constants.NoAlias)
            {
                sb.Append('-');
                sb.Append(desc.Alias);
            }

            if (desc.Name != Constants.NoName && (!shortForm || sb.Length == 0))
            {
                if (sb.Length != 0)
                    sb.Append(", ");

                sb.Append("--");
                sb.Append(desc.Name);
            }

            if (!desc.IsFlag)
            {
                if (shortForm || desc.ValueName != null)
                {
                    var value = desc.ValueName ?? "value";
                    sb.Append(" <");
                    sb.Append(value);

                    if (desc.IsList)
                        sb.Append("...");

                    sb.Append('>');
                }
            }

            return sb.ToString();
        }

        void PrintValue(string name, string helpText, int alignSize, int indentSize)
        {
            const int minIndentToHelpText = 2;

            _writer.Write(_align, 0, indentSize);
            _writer.Write(name);

            if (string.IsNullOrEmpty(helpText))
            {
                _writer.WriteLine();
                return;
            }

            var currentAlign = alignSize - indentSize - name.Length - minIndentToHelpText;
            if (currentAlign <= 0)
            {
                _writer.WriteLine();
                currentAlign = alignSize;
            }

            int partLen;
            for (int i = 0; i < helpText.Length; i += partLen)
            {
                _writer.Write(_align, 0, currentAlign);
                currentAlign = alignSize;

                partLen = GetNextWrappedPartLength(helpText, alignSize, i);
                _writer.WriteLine(helpText.AsSpan(i, partLen));
            }
        }

        int GetNextWrappedPartLength(string str, int alignSize, int index)
        {
            var totalLen = _outputWidth - alignSize - 1;
            var len = str.Length - index;
            if (len <= totalLen)
                return len;

            var ind = str.LastIndexOf(' ', index + totalLen, totalLen);
            return ind < 0 ? len : ind - index + 1;
        }
    }
}