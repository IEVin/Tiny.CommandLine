using System.TinyCommandLine.Implementation;

namespace System.TinyCommandLine.Samples
{
    public class SampleMultiCommands
    {
        // Examples:
        //  calc.exe add 1 7
        //  calc.exe sub 5 3
        //  calc.exe eval "sqrt((2+2)*2)"
        public static void EntryPoint(string[] args)
        {
            new CommandLineParser()
                .Command("add", builder => builder
                    .Option(out double num1, b => b.Required())
                    .Option(out double num2, b => b.Required())
                    .Invoke(() => Sub(num1, num2))
                )
                .Command("add", builder => builder
                    .Option(out double num1, b => b.Required())
                    .Option(out double num2, b => b.Required())
                    .Invoke(() => Sum(num1, num2))
                )
                .Command("eval", EvalCommand.Declare)
                .Run();
        }

        private static void Sum(double a, double b) => Console.WriteLine(a + b);
        private static void Sub(double a, double b) => Console.WriteLine(a - b);
    }

    // All command definition in a separated file
    public class EvalCommand
    {
        public static void Declare(CommandBuilder builder)
        {
            builder
                .HelpText("Evaluate complex expression")
                .Option(out string expr, b => b.Required())
                .Invoke(() => Handler(expr));
        }

        private static void Handler(string expr)
        {
            // ... eval command implementation ...
        }
    }
}