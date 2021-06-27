using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System.TinyCommandLine.Implementation
{
    readonly struct DefaultHelpBuilder : IHelpBuilder
    {
        readonly char[] _align;
        readonly TextWriter _writer;

        readonly int _outputWidth;
        readonly int _alignSize;

        public DefaultHelpBuilder(TextWriter writer, int alignSize)
        {
            _writer = writer;
            _outputWidth = Console.IsOutputRedirected ? int.MaxValue : Math.Max(10, Console.WindowWidth);
            _alignSize = Math.Min(alignSize, _outputWidth - 5);

            _align = new char[alignSize];
            Array.Fill(_align, ' ');
        }

        public void Show(string name, string helpText, List<string> commandParts, List<CommandDesc> commands, List<OptionDesc> options)
        {
            if (helpText != null)
            {
                _writer.WriteLine(helpText);
                _writer.WriteLine();
            }

            ShowSyntax(name, commandParts, commands, options);

            ShowCommands(commands);

            ShowOptions(options);
        }

        void ShowSyntax(string fileName, List<string> commandParts, List<CommandDesc> commands, List<OptionDesc> options)
        {
            if (options == null && commands == null)
                return;

            _writer.Write("Usage: ");
            _writer.Write(fileName);

            foreach (var part in commandParts)
            {
                _writer.Write(' ');
                _writer.Write(part);
            }

            int argumentNum = 0;
            if (options != null)
            {
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
            }

            if (commands != null)
            {
                _writer.Write(" <command> [args]");
            }

            _writer.WriteLine();
            _writer.WriteLine();
        }

        void ShowCommands(List<CommandDesc> commands)
        {
            if (commands == null)
                return;

            foreach (var desc in commands)
            {
                PrintValue(desc.Name, desc.HelpText, 2);
            }

            Console.WriteLine();
        }

        void ShowOptions(List<OptionDesc> options)
        {
            if (options == null)
                return;

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
                PrintValue(name, desc.HelpText, 4);
            }
        }

        string GetOptionName(OptionDesc desc, int argumentNum, bool shortForm)
        {
            // format "   -s, --long <value>"
            var len = 12 + (desc.LongName?.Length ?? 0) + (desc.ValueName?.Length ?? 0);
            var sb = new StringBuilder(len);

            // argument
            if (desc.IsArgument)
            {
                var argument = desc.ValueName ?? "argument" + argumentNum;
                sb.Append(argument);
                return sb.ToString();
            }

            // option
            if (desc.ShortName != '\0')
            {
                sb.Append('-');
                sb.Append(desc.ShortName);
            }

            if (desc.LongName != null && (!shortForm || sb.Length == 0))
            {
                if (sb.Length != 0)
                    sb.Append(", ");

                sb.Append("--");
                sb.Append(desc.LongName);
            }

            if (!desc.IsFlag)
            {
                if (shortForm || desc.ValueName != null)
                {
                    var value = desc.ValueName ?? "value";
                    sb.Append(" <");
                    sb.Append(value);
                    sb.Append('>');
                }
            }

            return sb.ToString();
        }

        void PrintValue(string name, string helpText, int intend)
        {
            const int intendBetweenNameAndText = 2;

            _writer.Write(_align, 0, intend);
            _writer.Write(name);

            if (string.IsNullOrEmpty(helpText))
            {
                _writer.WriteLine();
                return;
            }

            var currentAlign = _alignSize - intend - name.Length - intendBetweenNameAndText;
            if (currentAlign <= 0)
            {
                _writer.WriteLine();
                currentAlign = _alignSize;
            }

            int partLen;
            for (int i = 0; i < helpText.Length; i += partLen)
            {
                _writer.Write(_align, 0, currentAlign);
                currentAlign = _alignSize;

                partLen = GetNextWrappedPartLength(helpText, i);
                _writer.WriteLine(helpText.AsSpan(i, partLen));
            }
        }

        int GetNextWrappedPartLength(string str, int index)
        {
            var totalLen = _outputWidth - _alignSize - 1;
            var len = str.Length - index;
            if (len <= totalLen)
                return len;

            var ind = str.LastIndexOf(' ', index + totalLen, totalLen);
            return ind < 0 ? len : ind - index + 1;
        }
    }
}