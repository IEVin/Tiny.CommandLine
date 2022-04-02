$root = Resolve-Path -LiteralPath (Join-Path "${PSScriptRoot}" "..")

$version = if ($env:VERSION_OVERRIDE) { $env:VERSION_OVERRIDE } else { "0.0.0" };

dotnet run --project "$root/src/tools/SourceComposer" -- -f -o "$root/build/publish/Tiny.CommandLine.cs" --header "$root/src/Tiny.CommandLine/InfoHeader.cs" "$root/src/Tiny.CommandLine" --set "Version=$version"
