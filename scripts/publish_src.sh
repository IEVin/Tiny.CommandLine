#!/usr/bin/env bash
set +x  # echo off

ROOT=${0%/*}/..

dotnet run --project "$ROOT/src/tools/SourceComposer" -- -f -o "$ROOT/build/publish/Tiny.CommandLine.cs" --header "$ROOT/src/TinyCommandLine/InfoHeader.cs" "$ROOT/src/TinyCommandLine"
