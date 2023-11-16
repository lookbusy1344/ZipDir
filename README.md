# ZipDir

[![CodeQL](https://github.com/lookbusy1344/ZipDir/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/lookbusy1344/ZipDir/actions/workflows/github-code-scanning/codeql)

A small command line utility to list the contents of ZIP files. Given a folder, it recursively searches for ZIP files and lists their contents. This includes nested ZIP files.

```
ZipDir.exe -f C:\path

C:\path\to\zipfile.zip/contents1.txt
C:\path\to\zipfile.zip/contents2.txt
C:\path\to\zipfile.zip/Nested.zip/contents3.txt
C:\path\to\zipfile.zip/Nested.zip/contents4.txt
```


## Usage

```
Usage: ZipDir.exe [options]

Options:
  -f, --folder <path>   Folder to search (default ".")
  -p, --pattern <str>   Zip file pattern (default "*.zip")
  -e, --exclude <str>   Exclude patterns, can be specified multiple times "-e backup -e documents"
  -r, --raw             Raw output, for piping
  -h, --help, -?        Help information

Example:
  ZipDir.exe -f .
  ZipDir.exe --folder \your\docs --pattern *.zip --exclude backup --exclude documents
```

## Build

ZipDir is written in C# and built with .NET 8. Build with

```
dotnet publish ZipDir.csproj -r win-x64 -c Release
```

Or use the supplied `build.cmd` file.

The csprog file is configured for Native AOT compilation, but this is optional.
