$root = Resolve-Path -LiteralPath (Join-Path "${PSScriptRoot}" "..")

dotnet remove "$root/src/Tests/Tests.csproj" reference "$root/src/TinyCommandLine/TinyCommandLine.csproj"
dotnet restore
dotnet add "$root/src/Tests" package -s "$root/build/publish" Tiny.CommandLine
