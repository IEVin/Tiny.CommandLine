using System;

namespace Tiny.CommandLine.Example
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