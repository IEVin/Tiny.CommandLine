using System;

namespace Tiny.CommandLine.Sample
{
    public class TestCommand
    {
        public static readonly string HelpText = "Show test message";

        public static void Declare(CommandLineParser parser)
        {
            parser.Run();

            Console.WriteLine("Test");
        }
    }
}