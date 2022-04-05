using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tiny.CommandLine;

const int maxNestedNameSpaceLevel = 10;
const int emptyLinesCountBetweenFiles = 1;
const string whitespaces = "\t ";

new CommandLineParser(args, "compose")
    .Option('o', "output", out string output, "Path to output file with combined sources")
    .Option('f', "force", out bool force, "Force override output file if exist")
    .Option("header", out string header, "Path to file with usage and license header")
    .Option("indent", out string indent, "Characters that will be used as indentation", () => "    ")
    .OptionList<string>('s', "set", out var overrides, "Replace variable to value in a header text", valueName: "name=value")
    .Argument(out string input, "Path to directory with sources", required: true, valueName: "path")
    .Check(() => Directory.Exists(input), $"Directory '{input}' is not found")
    .Check(() => header == null || File.Exists(header), $"Header file '{header}' is not found")
    .Check(() => output == null || force || !File.Exists(output), $"Output file '{output}' is already exist")
    .Run();

ComposeHandler(input, output, header, indent, overrides);


static void ComposeHandler(string input, string output, string header, string indent, IReadOnlyList<string> overrides)
{
    var overridesList = (overrides ?? Array.Empty<string>())
        .Select(x => x.Split('=', 2, StringSplitOptions.RemoveEmptyEntries))
        .Where(x => x.Length == 2)
        .Select(x => ($"$({x[0]})", x[1]))
        .ToList();

    var usingList = new HashSet<string>();
    var sourcesDict = new Dictionary<string, List<string>>();

    var sources = Directory.GetFiles(input, "*.cs", SearchOption.AllDirectories);
    Array.Sort(sources);

    foreach (var path in sources)
    {
        ParseSourceFile(path, out var nsName, out var content, usingList);

        if (content.Count == 0)
            continue;

        // Group sources by namespaces
        var sourcesByNs = GetValueOrAddDefault(sourcesDict, nsName);
        sourcesByNs.Capacity += content.Count + emptyLinesCountBetweenFiles;

        // Add some empty lines between sources from different files
        int emptyLinesCount = sourcesByNs.Count > 0 ? emptyLinesCountBetweenFiles : 0;
        while (emptyLinesCount-- > 0)
        {
            sourcesByNs.Add(string.Empty);
        }

        // add content
        sourcesByNs.AddRange(content);
    }

    var headerContent = header != null
        ? File.ReadLines(header)
        : null;

    SaveCombinedContent(output, headerContent, usingList, sourcesDict, indent, overridesList);

    Console.WriteLine($"Result: {Path.GetFullPath(output)}");
}

static TValue GetValueOrAddDefault<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key)
    where TValue : new()
{
    return dict.TryGetValue(key, out var value)
        ? value
        : dict[key] = new TValue();
}

static void ParseSourceFile(string path, out string nsName, out List<string> content, HashSet<string> usingList)
{
    content = new List<string>();
    nsName = string.Empty;

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
                nsName = trimmed
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

static void SaveCombinedContent(string path, IEnumerable<string> headerContent, HashSet<string> usingList,
    Dictionary<string, List<string>> sourcesDict, ReadOnlySpan<char> indent, IReadOnlyList<(string, string)> overrides)
{
    var outputDir = Path.GetDirectoryName(path);
    if (outputDir != null)
    {
        Directory.CreateDirectory(outputDir);
    }

    using var stream = path != null
        ? File.Create(path)
        : Console.OpenStandardOutput();

    var output = new StreamWriter(stream, new UTF8Encoding(false, true), 8 * 1024);

    // license and usage header
    if (headerContent != null)
    {
        foreach (var q in headerContent)
        {
            var line = q;
            foreach (var (name, val) in overrides)
            {
                if (!line.Contains(name))
                    continue;

                line = line.Replace(name, val);
            }

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

    using var owner = MemoryPool<char>.Shared.Rent(indent.Length * 10);
    var buffer = owner.Memory.Span;
    for (int i = 0; i < maxNestedNameSpaceLevel; i++)
    {
        indent.CopyTo(buffer.Slice(i * indent.Length));
    }

    var nestedNs = new Stack<string>();

    var orderedSources = sourcesDict.OrderBy(x => x.Key);
    foreach (var item in orderedSources)
    {
        var ns = item.Key;

        while (nestedNs.TryPeek(out var parentNs))
        {
            if (ns.Length > parentNs.Length && ns[parentNs.Length] == '.' && ns.StartsWith(parentNs))
            {
                ns = ns.Substring(parentNs.Length + 1);
                break;
            }

            output.Write(buffer.Slice(0, nestedNs.Count * indent.Length));
            output.WriteLine('}');
            nestedNs.Pop();
        }

        var currentIndent = buffer.Slice(0, nestedNs.Count * indent.Length);

        output.WriteLine();
        output.WriteLine();

        output.Write(currentIndent);
        output.WriteLine($"namespace {ns}");

        output.Write(currentIndent);
        output.WriteLine('{');

        foreach (var content in item.Value)
        {
            if (content.Length > 0)
                output.Write(currentIndent);

            output.WriteLine(content);
        }

        nestedNs.Push(item.Key);
    }

    for (int i = nestedNs.Count - 1; i >= 0; i--)
    {
        output.Write(buffer.Slice(0, i * indent.Length));
        output.WriteLine('}');
    }

    output.Flush();
}