using System;
using System.Collections.Generic;
using System.IO;
using Tiny.CommandLine;
using Tiny.CommandLine.Sample;


CommandLineParser.Run("sample", args, syntax => syntax
    .HelpText("This sample is some variation of 'echo'")

    // Firstly should be declared global options. This options must not be repeated in subcommands
    .Option('v', "verbose", out bool verbose, "Be more verbose")

    // Secondly should be declared subcommands
    .Command("show", s1 => s1
        .HelpText("Show some info")

        // Subcommands can declare theirs own subcommands
        .Command("version", s2 => s2
            .HelpText("Show version")

            // Command can declare handler like this
            .Handler(() => ShowVersion(verbose))
        )

        // In ideal subcommands command should be declared in separate class in one line
        .Command("test", TestCommand.Declare)
    )

    // Params structure can be created in all places
    .Variable(out var args, new SampleArguments())

    // Futher should be declared command specific options
    .Option('f', out bool force, s => s
        .HelpText("Override output file")
    )
    .Option("output", out args.OutputPath, s => s
        .HelpText("Path to output file")
        .ValueName("path")
        .Default(string.Empty)
    )

    // Options with multiple usage should be declarad as OptionList
    .OptionList('I', "include", out args.Includes, "Only items starts with specified prefix should be printed")
    .OptionList('X', "exclude", out args.Excludes, "Items starts with specified prefix will be ignored")

    // After options should be declared arguments
    .Argument(out string text, s => s
        .Required()
    )

    // Arguments with multiple usage should be declarad as ArgumentList. It should be declared after all arguments
    .ArgumentList(out args.OtherArguments, s => s
        .HelpText("Others arguments")
        .ValueName("others")
    )

    // To validate options in command can be declared with Check
    .Check(() => force || !File.Exists(args.OutputPath), $"File '{args.OutputPath}' is already exist. Use --force to override it")

    // Finally all commands that can be invoked without subcomands should declare handler
    .Handler(() => SampleHandler(args, text))
);

static void ShowVersion(bool verbose)
{
    if (verbose)
        Console.Write("sample version ");
    Console.WriteLine("0.0.1");
}

static void SampleHandler(SampleArguments args, string content)
{
    // ... implementation ...

    Console.WriteLine(content);
}


class SampleArguments
{
    public string OutputPath;

    public IReadOnlyList<string> Includes;
    public IReadOnlyList<string> Excludes;

    public IReadOnlyList<string> OtherArguments;
}