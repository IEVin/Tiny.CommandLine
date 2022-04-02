$root = Resolve-Path -LiteralPath (Join-Path "${PSScriptRoot}" "..")

dotnet remove "$root/src/Tests/Tests.csproj" reference "$root/src/Tiny.CommandLine/Tiny.CommandLine.csproj"
Copy-Item -Force -Path "$root/build/publish/Tiny.CommandLine.cs" -Destination "$root/src/Tests/"
