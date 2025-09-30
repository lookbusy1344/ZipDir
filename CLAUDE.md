# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ZipDir is a command-line utility that recursively searches for ZIP files in a directory and lists their contents, including nested ZIP files. The application is built with .NET 9.0 and uses Native AOT compilation for performance.

## Development Commands

### Build and Publish
```bash
# Clean the project
dotnet clean --verbosity minimal

# Build the project
dotnet build

# Publish for Windows x64
dotnet publish ZipDir.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishAot=false --self-contained false

# Publish for macOS Apple Silicon
dotnet publish ZipDir.csproj -c Release -r osx-arm64 -p:PublishSingleFile=true -p:PublishAot=false --self-contained false

# Publish for Windows ARM64
dotnet publish ZipDir.csproj -c Release -r win-arm64 -p:PublishSingleFile=true -p:PublishAot=false --self-contained false
```

### Security and Analysis
```bash
# Check for vulnerabilities
dotnet restore
dotnet list package --vulnerable --include-transitive
```

### Testing
```bash
# Run automated tests
dotnet test
```

### Code Style and Analysis
```bash
# Format code according to .editorconfig rules (run after making changes)
dotnet format ZipDir.csproj
dotnet format ZipDir.Tests/ZipDir.Tests.csproj
```

The project enforces strict code analysis with Roslynator and other analyzers. All analysis modes are enabled:
- Design, Security, Performance, Reliability, Usage analysis are all set to "All"
- EnforceCodeStyleInBuild is enabled
- Uses comprehensive .editorconfig with C# coding conventions
- **Always run `dotnet format` after making code changes to ensure consistent formatting**

## Architecture

### Core Components

1. **Program.cs** - Entry point and command-line parsing using PicoArgs
2. **Config.cs** - Configuration record (`ZipDirConfig`) and `HelpException`
3. **Searcher.cs** - Core search functionality for finding and processing ZIP files
4. **ZipUtils.cs** - ZIP file detection utilities (by extension or magic number)
5. **GitVersion.cs** - Version information handling
6. **PicoArgs.cs** - Command-line argument parsing library

### Key Features
- Supports both extension-based and magic number-based ZIP detection
- Multi-threaded and single-threaded processing modes
- Raw output mode for piping
- Exclude patterns support
- Nested ZIP file processing

### Code Conventions
- Uses file-scoped namespaces
- Comprehensive .editorconfig with strict formatting rules
- Tab character indentation for CS files
- CRLF line endings for CS files. Other files like markdown use LF.
- Braces on same line for control blocks, new line for methods/types
- Extensive analyzer rules configured in .editorconfig

### Dependencies
- **PicoArgs-dotnet**: Command-line argument parsing
- **Native AOT**: For performance optimization
- **Multiple analyzers**: Roslynator, RecordValueAnalyser, Threading analyzers

## Project Structure
- Single executable project targeting .NET 9.0
- Unit test project (ZipDir.Tests) with comprehensive test coverage
- Uses record types for configuration with value semantics
- Implements custom equality for collections in records
