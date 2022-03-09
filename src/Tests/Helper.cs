using System.Collections.Generic;
using System.Text;

namespace Tiny.CommandLine.Tests
{
    public static class Helper
    {
        public static void Run(string cmd, CommandConfigurator configure)
        {
            var args = SplitArguments(cmd);
            CommandLineParser.Run("test", args, configure);
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
}