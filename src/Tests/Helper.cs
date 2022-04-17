using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace Tiny.CommandLine.Tests
{
    public static class Helper
    {
        public static CommandLineParser CreateParser(string cmd)
        {
            var args = SplitArguments(cmd);
            return new CommandLineParser(args, "test");
        }

        public static void Run(string cmd, Action<CommandLineParser> configure)
        {
            var parser = CreateParser(cmd);
            configure(parser);

            var result = parser.GetResult();
            Assert.IsTrue(result);
        }

        public static void OverrideOutput()
        {
            Console.SetError(TextWriter.Null);
            Console.SetOut(TextWriter.Null);
        }

        public static void OverrideExit()
        {
            Action<int> onExit = x => throw new ExitException(x);

            var exitAction = typeof(ParserExtensions).GetField("ExitAction", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(exitAction);
            exitAction.SetValue(null, onExit);
        }

        static string[] SplitArguments(string commandline)
        {
            var list = new List<string>();

            bool isEscaped = false;
            int quoteCount = 0;

            var sb = new StringBuilder(commandline.Length);

            for (int i = 0; i < commandline.Length; i++)
            {
                var ch = commandline[i];

                if (isEscaped)
                {
                    sb.Append(ch);
                    isEscaped = false;
                    continue;
                }

                if (ch == '\\')
                {
                    isEscaped = true;
                    continue;
                }

                if (ch == '"')
                {
                    if (quoteCount > 0 && commandline[i - 1] == '"')
                    {
                        quoteCount--;
                        sb.Append('"');
                        continue;
                    }

                    quoteCount++;
                    continue;
                }

                if (quoteCount != 1 && ch == ' ')
                {
                    quoteCount = 0;
                    list.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }

                if (quoteCount > 1)
                {
                    quoteCount = 0;
                    list.Add(sb.ToString());
                    sb.Clear();
                }

                sb.Append(ch);
            }

            if (sb.Length > 0)
            {
                list.Add(sb.ToString());
            }

            return list.ToArray();
        }
    }

    class ExitException : Exception
    {
        public int Code { get; private set; }

        public ExitException(int code) => Code = code;
    }
}