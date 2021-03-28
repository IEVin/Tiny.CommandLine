using System.Collections.Generic;
using System.Text;

namespace System.TinyCommandLine.Tests
{
    public static class Helper
    {
        public static void Run(string cmd, Implementation.CommandConfigurator configure)
        {
            var args = SplitArguments(cmd);
            CommandLineParser.Run("test", args, configure);
        }

        static string[] SplitArguments(string commandline)
        {
            var list = new List<string>();

            bool isEscaped = false;
            int quoteIndex = int.MinValue;
            var sb = new StringBuilder(commandline.Length);

            for (int i = 0; i < commandline.Length; i++)
            {
                var ch = commandline[i];

                if (isEscaped)
                {
                    sb.Append(ch);
                    continue;
                }

                if (ch == '\\')
                {
                    isEscaped = true;
                    continue;
                }

                if (ch == '"')
                {
                    if (quoteIndex == i - 1)
                    {
                        quoteIndex = int.MinValue;
                        sb.Append('"');
                        continue;
                    }

                    quoteIndex = i;
                    continue;
                }

                if (quoteIndex < 0 && ch == ' ')
                {
                    list.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }

                sb.Append(ch);
            }

            list.Add(sb.ToString());
            return list.ToArray();
        }
    }
}