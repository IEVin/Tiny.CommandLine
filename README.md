Tiny.CommandLine [![Build Status](https://github.com/IEVin/Tiny.CommandLine/actions/workflows/main-ci.yml/badge.svg)](https://github.com/IEVin/Tiny.CommandLine/actions/workflows/main-ci.yml)[![Nuget](https://img.shields.io/nuget/v/Tiny.CommandLine)](https://www.nuget.org/packages/Tiny.CommandLine)
=====================

The **Tiny Command Line** is a small but full-featured command line parser for C#. 

### Key features
- Compact python-like API
- No depedencies
- No reflection (no hidden metadata usage in `System.Convert` and others)
- Single file distribution ([**Tiny.CommandLine.cs**](https://github.com/IEVin/Tiny.CommandLine/releases/latest/download/Tiny.CommandLine.cs) published with [releases](https://github.com/IEVin/Tiny.CommandLine/releases/))

Why another one parser?
---------------------
Most console applications require only a small number of parameters. In this case declaring a bulky structure with attributes would be redundant.
The **Tiny Command Line** uses a simple python-like syntax that allows to declare one commandline argument in one line.
It also don't use reflection and helps to avoid problems with trimming application and compiling to the native view.

Usage
---------------------

```CSharp
using Tiny.CommandLine;

new CommandLineParser(args, "sample")
    .Option('i', "input", out string input, "The path to input file", required: true)
    .Option("output", out string output, "The path to output file")
    .Run();

/* ... main code ... */
```

The help of this simple exampe will look like this

```
>example.exe -h

Usage: example [-i <value>] --output <value>

    -i, --input             The path to input file
    --output                The path to output file
```

To see a detailed example with all features take a look at [src/Sample/Program.cs](https://github.com/IEVin/Tiny.CommandLine/blob/master/src/Sample/Program.cs).

### Supported features
- Options (with names and/or alias) and arguments
- Commands and sub-commands (no depth limit)
- Global options applied to all commands (just defined before commands)
- Help (`-h` and `--help`)
- Types: Numeric (all signed/unsigned/float), bool, char, string, DateTime, Nullable&lt;T&gt;
- Multiple options and arguments occurrence (like `-e one -e two -e three`)
- Сustom type conversion

### Not supported features (but in the plans)
- Enums out of the box. Can be parsed with custom converter.
- Collections (like `-e one two three`). Can be parsed as a `OptionList`
- Version (`-v` and `--verions`)
- Async command handlers
