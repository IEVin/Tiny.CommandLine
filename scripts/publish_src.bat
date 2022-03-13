@echo off

set ROOT=%~dp0..

dotnet run --project "%ROOT%\src\tools\SourceComposer" -- -f -o "%ROOT%\build\publish\Tiny.CommandLine.cs" --header "%ROOT%\src\TinyCommandLine\InfoHeader.cs" "%ROOT%\src\TinyCommandLine"
