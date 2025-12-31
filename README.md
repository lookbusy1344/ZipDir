# ZipDir

[![CodeQL](https://github.com/lookbusy1344/ZipDir/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/lookbusy1344/ZipDir/actions/workflows/github-code-scanning/codeql)

A command-line utility built with .NET 10.0 that recursively searches for ZIP files in a directory and lists their contents, including nested ZIP files. The application uses Native AOT compilation for optimal performance.

```
ZipDir -f C:\path

C:\path\to\zipfile.zip/contents1.txt
C:\path\to\zipfile.zip/contents2.txt
C:\path\to\zipfile.zip/Nested.zip/contents3.txt
C:\path\to\zipfile.zip/Nested.zip/contents4.txt
```

## Usage

```
Usage: ZipDir [options]

Options:
  -f, --folder <path>   Folder to search (default ".")
  -p, --pattern <str>   Zip file pattern (default "*.zip")
  -e, --exclude <str>   Exclude patterns, can be specified multiple times "-e backup -e documents"
  -b, --byte            Identify zip files by magic number, not extension
  -r, --raw             Raw output, for piping
  -s, --single-thread   Use a single thread for processing
  -h, --help, -?        Help information

Example:
  ZipDir -f .
  ZipDir --folder \your\docs --pattern *.zip --exclude backup --exclude documents
```

## Testing

The project includes comprehensive unit and integration tests:

```
dotnet test
```

## Build project for deployment

Build on Windows x64:

```
dotnet publish ZipDir.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishAot=false --self-contained false
```

And on MacOS (Apple Silicon):

```
dotnet publish ZipDir.csproj -c Release -r osx-arm64 -p:PublishSingleFile=true -p:PublishAot=false --self-contained false
```

## Requirements

- .NET 10.0 SDK or later

**Version History:**
- Commit `103da14899a389df367a327eab91594cb21d962a` and later: .NET 10.0 support
- Earlier commits: .NET 9.0

## Features

- Multi-threaded and single-threaded processing modes
- ZIP file detection by extension or magic number
- Pattern-based file filtering
- Exclude patterns support
- Raw output mode for piping to other tools
- Comprehensive test coverage with 27+ unit and integration tests
- Strict code analysis with Roslynator and multiple analyzers
