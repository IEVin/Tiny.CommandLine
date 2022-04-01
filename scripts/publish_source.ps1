$root = Resolve-Path -LiteralPath (Join-Path "${PSScriptRoot}" "..")

dotnet run --project "$root/src/tools/SourceComposer" -- -f -o "$root/build/publish/Tiny.CommandLine.cs" --header "$root/src/TinyCommandLine/InfoHeader.cs" "$root/src/TinyCommandLine"
