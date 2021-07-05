using System;

namespace Tiny.CommandLine.Sample
{
    public class TestCommand
    {
        public static void Declare(CommandBuilder syntax)
        {
            syntax
                .HelpText("Show test message")
                .Handler(Handler);
        }

        private static void Handler() => Console.WriteLine("Test");
    }
}