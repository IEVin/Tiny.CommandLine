using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tiny.CommandLine;

const int emptyLinesCountBetweenFiles = 1;
const string whitespaces = "\t ";

CommandLineParser.Run("SourceComposer", args, b => b
    .Option('o', "output", out string output, "Path to output file with combined sources")
    .Option('f', "force", out bool force, "Force override output file if exist")
    .Option("header", out string header, "Path to file with usage and license header")
    .Argument(out string input, s => s
        .HelpText("Path to directory with sources")
        .ValueName("path")
        .Required()
    )
    .Check(() => Directory.Exists(input), $"Directory '{input}' is not found")
    .Check(() => header == null || File.Exists(header), $"Header file '{header}' is not found")
    .Check(() => output == null || force || !File.Exists(output), $"Output file '{output}' is already exist")
    .Handler(() => ComposeHandler(input, output, header)));


static void ComposeHandler(string input, string output, string header)
{
    var usingList = new HashSet<string>();
    var sourcesDict = new Dictionary<string, List<string>>();

    var sources = Directory.GetFiles(input, "*.cs", SearchOption.AllDirectories);
    foreach (var path in sources)
    {
        ParseSourceFile(path, out var @namespace, out var content, usingList);

        if (content.Count == 0)
            continue;

        // Group sources by namespaces
        List<string> sourcesByNS;
        if (!sourcesDict.TryGetValue(@namespace, out sourcesByNS))
        {
            sourcesByNS = new();
            sourcesDict.Add(@namespace, sourcesByNS);
        }

        sourcesByNS.Capacity += content.Count + emptyLinesCountBetweenFiles;

        // Add some empty lines between sources from different files
        int emptyLinesCount = sourcesByNS.Count > 0 ? emptyLinesCountBetweenFiles : 0;
        while (emptyLinesCount-- > 0)
        {
            sourcesByNS.Add(string.Empty);
        }

        // add content
        sourcesByNS.AddRange(content);
    }

    var headerContent = header != null
        ? File.ReadLines(header)
        : null;

    SaveCombinedContent(output, headerContent, usingList, sourcesDict);
}

static void ParseSourceFile(string path, out string @namespace, out List<string> content, HashSet<string> usingList)
{
    content = new List<string>();
    @namespace = string.Empty;

    bool isFirstLine = true;
    bool inNamespace = false;

    var lines = File.ReadLines(path);
    foreach (var line in lines)
    {
        var span = line.AsSpan();

        // remove only comments started with double slashes (not triple)
        var commentInd = GetIndexOfToken(line, "//");
        if (commentInd >= 0 && !line.AsSpan(commentInd).StartsWith("/// "))
        {
            span = span.Slice(0, commentInd);
        }

        // skip lines that stay empty after remove comments
        var trimmed = span.TrimStart(whitespaces);
        if (trimmed.Length == 0 && commentInd >= 0)
            continue;

        // collect using directives
        if (trimmed.StartsWith("using"))
        {
            var ns = trimmed
                .Slice("using".Length)
                .TrimStart(whitespaces)
                .TrimEnd("\t ;");

            usingList.Add(ns.ToString());
            continue;
        }

        // collect namespaces
        if (!inNamespace)
        {
            if (trimmed.StartsWith("namespace"))
            {
                @namespace = trimmed
                    .Slice("namespace".Length)
                    .Trim(whitespaces)
                    .ToString();

                inNamespace = true;
            }

            continue;
        }

        // remove first line with '{'
        if (isFirstLine && trimmed.StartsWith("{"))
        {
            isFirstLine = false;
            continue;
        }

        content.Add(span.ToString());
    }

    if (content.Count == 0)
        return;

    // remove trailing empty lines and last line (assume that it is '}')
    int endIndex = content.Count - 1;
    for (; endIndex >= 0 && content[endIndex].Length == 0; endIndex--) {}

    content.RemoveRange(endIndex, content.Count - endIndex);
}

static int GetIndexOfToken(string line, string value)
{
    var charInd = -1;
    while ((charInd = line.IndexOf(value, charInd + 1, StringComparison.Ordinal)) >= 0)
    {
        bool isInString = false;

        var quoteInd = -1;
        while ((quoteInd = line.IndexOf('"', quoteInd + 1, charInd - quoteInd)) >= 0)
        {
            isInString ^= quoteInd > 0 && line[quoteInd - 1] != '\\' && line[quoteInd - 1] != '\'';
        }

        if (!isInString)
            return charInd;
    }

    return -1;
}

static void SaveCombinedContent(string path, IEnumerable<string> headerContent, HashSet<string> usingList, Dictionary<string, List<string>> sourcesDict)
{
    using var stream = path != null
        ? File.OpenWrite(path)
        : Console.OpenStandardOutput();

    var output = new StreamWriter(stream, new UTF8Encoding(false, true), 8 * 1024);

    // license and usage header
    if (headerContent != null)
    {
        foreach (var line in headerContent)
        {
            output.WriteLine(line);
        }

        output.WriteLine();
    }

    // using directives
    var orderedUsingList = usingList.OrderBy(x => x);
    foreach (var token in orderedUsingList)
    {
        output.WriteLine($"using {token};");
    }

    // namespaces with content
    var orderedSources = sourcesDict.OrderBy(x => x.Key);
    foreach (var @namespace in orderedSources)
    {
        output.WriteLine();
        output.WriteLine();
        output.WriteLine($"namespace {@namespace.Key}");
        output.WriteLine('{');

        foreach (var content in @namespace.Value)
        {
            output.WriteLine(content);
        }

        output.WriteLine('}');
    }
}
