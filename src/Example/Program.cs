using System;
using System.Collections.Generic;
using System.IO;
using Tiny.CommandLine;
using Tiny.CommandLine.Example;


new CommandLineParser(args, "example", "This example is some variation of 'echo'")
    // Firstly should be declared global options. This options must not be repeated in sub-commands
    .Option('v', "verbose", out bool verbose, "Be more verbose")

    // Secondly should be declared sub-commands
    .Command("show", "Show some info", s1 => s1

        // Sub-commands can declare theirs own sub-commands
        .Command("version", "Show version", s2 =>
        {
            // When all parameters are declared the handler must invoke Run (or GetResult)
            s2.Run();

            ShowVersion(verbose);
        })

        // In ideal sub-commands command should be declared in separate class in one line
        .Command("test", TestCommand.HelpText, TestCommand.Declare)
    )

    // Params structure can be created in all places
    .Variable(out var prm, new ExampleArguments())

    // Further should be declared command specific options
    .Option('f', out bool force, "Override output file")
    .Option("output", out prm.OutputPath, "Path to output file", () => string.Empty, valueName: "path")

    // Options with multiple usage should be declared as OptionList
    .OptionList('I', "include", out prm.Includes, "Only items starts with specified prefix should be printed")
    .OptionList('X', "exclude", out prm.Excludes, "Items starts with specified prefix will be ignored")

    // After options should be declared arguments
    .Argument(out string text, "Text to print", required: true)

    // Arguments with multiple usage should be declared as ArgumentList. It should be declared after all arguments
    .ArgumentList(out prm.OtherArguments, "Others arguments", valueName: "others")

    // To validate options in command can be declared with Check
    .Check(() => force || !File.Exists(prm.OutputPath), $"File '{prm.OutputPath}' is already exist. Use --force to override it")

    // Finally after all parser should invoke Run (or GetResult)
    .Run();

ExampleHandler(prm, text);


static void ShowVersion(bool verbose)
{
    if (verbose)
        Console.Write("example version ");
    Console.WriteLine("0.0.1");
}

static void ExampleHandler(ExampleArguments args, string content)
{
    // ... implementation ...

    Console.WriteLine(content);
}


class ExampleArguments
{
    public string OutputPath;

    public IReadOnlyList<string> Includes;
    public IReadOnlyList<string> Excludes;

    public IReadOnlyList<string> OtherArguments;
}