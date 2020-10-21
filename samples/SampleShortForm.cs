using System.IO;

namespace System.TinyCommandLine.Samples
{
    public class SampleShortForm
    {
        // Examples:
        //  grep.exe --expr "some text" --ignore-case -i input.txt -o output.txt
        //  grep.exe -e "some text" -i input.txt -o output.txt -v
        //  grep.exe -e "some text" --o "../new output.txt" --ignore-case
        //  grep.exe -e "[\W\w]{5,}" --regexp
        public static void EntryPoint(string[] args) =>
            CommandLineParser.Run(args, s => s
                .HelpText("Simple utility for searching text for lines that match a regular expression.")
                .Option('i', "input", out string input, "The file to search in.")
                .Option('o', "output", out string output, "The file to save search result.")
                .Option('e', "expr", out string expression, b => b
                    .HelpText("The file to save search result.")
                    .Required()
                )
                .Option('s', "--line-separator", out string separator, b => b
                    .HelpText("The line ending symbol.")
                    .Default("\n")
                )
                .Option("ignore-case", out bool ignoreCase, "Use case insensitive search.")
                .Option("regexp", out bool regexp, "Use regular expression for search.")
                .Option('v', "verbose", out bool verbose, b => b
                    .HelpText("Enable verbose mode.")
                    .Hidden()
                )
                .Check(() => !regexp || !ignoreCase, "Options --regexp and --ignore-case are not compatible.")
                .Check(() => input == null || File.Exists(input), $"The file '{input}' not found.")
                .Check(() => output == null || !File.Exists(output), $"The file '{output}' is already exists.")
                .Handler(() => GrepHandler(input, output, expression, separator, ignoreCase, regexp, verbose))
            );

        private static void GrepHandler(string input, string output, string expr, string separator, bool ignoreCase, bool useRegexp, bool verbose)
        {
            // ... grep command implementation ...
        }
    }
}