using System.Collections.Generic;

namespace System.TinyCommandLine.Samples
{
    public class SampleWithStruct
    {
        public class OptionsSet
        {
            public string Input;
            public string Output;
            public string Expression;
            public string Separator;
            public bool IgnoreCase;
            public bool Regexp;
        }

        // Examples:
        //  grep.exe --expr "some text" --ignore-case -i input.txt -o output.txt
        //  grep.exe -e "some text" -i input.txt -o output.txt -v
        //  grep.exe -e "some text" --o "../new output.txt" --ignore-case
        //  grep.exe -e "[\W\w]{5,}" --regexp
        public static void EntryPoint(string[] args) =>
            CommandLineParser.Run("grep", args, syntax => syntax
                .HelpText("Simple utility for searching text for lines that match a regular expression.")
                .Variable(out var x, new OptionsSet())
                .Option('i', "input", out x.Input, "The file to search in.")
                .Option('o', "output", out x.Output, "The file to save search result.")
                .Option('e', "expr", out x.Expression, b => b
                    .HelpText("The file to save search result.")
                    .Required()
                )
                .Option('s', "--line-separator", out x.Separator, b => b
                    .HelpText("The line ending symbol.")
                    .Default("\n")
                )
                .Option("ignore-case", out x.IgnoreCase, "Use case insensitive search.")
                .Option("regexp", out x.Regexp, "Use regular expression for search.")
                .Option<bool>('v', "verbose", out var verbose, b => b
                    .HelpText("Enable verbose mode.")
                    .Hidden()
                )
                .Check(() => !x.Regexp || !x.IgnoreCase, "Options --regexp and --ignore-case are not compatible.")
                .Handler(() => GrepHandler(x, verbose))
            );

        private static void GrepHandler(OptionsSet args, bool verbose)
        {
            // ... grep command implementation ...
        }
    }
}