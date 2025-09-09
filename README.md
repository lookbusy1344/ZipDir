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
  -b, --byte            Identify zip files by magic number, not extension
  -r, --raw             Raw output, for piping
  -h, --help, -?        Help information

Example:
  ZipDir.exe -f .
  ZipDir.exe --folder \your\docs --pattern *.zip --exclude backup --exclude documents
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
