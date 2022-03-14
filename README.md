TinyCommandLine [![Build Status](https://github.com/IEVin/TinyCommandLine/actions/workflows/main-ci.yml/badge.svg)](https://github.com/IEVin/TinyCommandLine/actions/workflows/main-ci.yml)
=====================

The **TinyCommandLine** is a small but full-featured command line parser for C#. 

### Key features
- Compact python-like API
- No depedencies
- No reflection (no hidden metadata usage in `System.Convert` and others)
- Single file distribution ([**Tiny.CommandLine.cs**](https://github.com/IEVin/TinyCommandLine/releases/latest/download/Tiny.CommandLine.cs) published with [releases](https://github.com/IEVin/TinyCommandLine/releases/))

Why another one parser?
---------------------
In most cases only a small number of options are sufficient for a console application. It is redundant to create a bulky structure with attributes. Also this works slowly because of we need to use reflection. It can also cause problems for trimming and compiling to the native view.

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

An example using all parser features can be found in [src/Sample/Program.cs](https://github.com/IEVin/TinyCommandLine/blob/master/src/Sample/Program.cs)

### Supported features
- Options (with names and/or alias) and arguments
- Commands and child commands (no depth limit)
- Global options applied to all commands (just defined before commands)
- Help (`-h` and `--help`)
- Types: Numeric (all signed/unsigned/float), bool, char, string, DateTime, Nullable<T>
- Multiple options and arguments occurrence (like `-e one -e two -e three`)
- Сustom type conversion

### Not supported features in the plans
- Enums out of the box. Can be parsed with custom converter.
- Collections (like `-e one two three`). Can be parsed as a `OptionList`
- Version (`-v` and `--verions`)
- Async command handlers
