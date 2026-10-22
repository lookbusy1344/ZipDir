# Project guidelines

This file provides guidance to coding agents working with code in this repository.

## Project Overview

ZipDir is a command-line utility that recursively searches for ZIP files in a directory and lists their contents, including nested ZIP files. The
application is built with .NET 10.0 and uses Native AOT compilation for performance.

## Version control (read first)

Before the first VCS command, run `jj --ignore-working-copy root` as a command of its own. Do not chain it with other commands: a later failure hides
its exit status. If it prints a path, the repo is jj. This may be a jj repo on one machine and plain Git on another.

**IMPORTANT:** if it succeeds, use `jj` for all VCS commands, including `log`, `show`, `status` and `diff`. Do not run `git` on jj repos. The
`gitStatus` snapshot in the session context is not a reason to use git.

jj has no commit hooks. Run `scripts/pre-commit.sh` before `jj commit`, `jj describe` (when finalising a change) and `jj squash`. It checks the files
changed in `@` and skips the dotnet steps when only non-code files (`*.md`) changed. In a plain Git checkout `.git/hooks/pre-commit` runs it.

jj does not apply `.gitattributes` line-ending conversion. A file with CRLF on disk shows as modified against an LF blob. Convert it to LF.

Before `jj git push` or moving a shared bookmark, run `scripts/pre-push.sh`. By default it formats the tip (newest non-empty mutable revision) with
`jj fix` (`scripts/format-stdin.sh`, whitespace rules), then runs restore, build, `dotnet format --verify-no-changes` and tests on it. `--full` formats
every mutable revision in `::@` and checks each in its own checkout via `jj run`. `scripts/agent-pre-push-hook.sh` runs the tip check on every push
command and blocks the push on failure. Claude Code (`.claude/settings.json`) and Codex (`.codex/hooks.json`) call it as a `PreToolUse` hook. The hook
checks the tip of `@`, not the bookmark being pushed, so push only the bookmark you are working on: the one on `@-`, with an empty `@` above it. In a
plain Git checkout it runs the same checks on the working tree without formatting it, and refuses uncommitted changes to tracked files so the checked
tree is the pushed HEAD. `git push` triggers the same hook.

Push only on explicit request. "Push this" means: if `@` is non-empty, `jj commit` it (after the pre-commit checks). Move the bookmark to `@-` with
`jj bookmark set <name> -r @-`, then `jj git push --bookmark <name>`. Use the bookmark already on the stack; otherwise `main`. Never push any other
bookmark. Do not use `jj git push -c`.

## Personal information

Exclude PII from every commit, commit message and bookmark name: real names, email addresses, usernames, machine paths such as `/Users/<name>/`,
hostnames, tokens and credentials. Check the diff before `jj commit`, `jj describe` (finalising) and `git commit`.

## Committing

`scripts/pre-commit.sh` runs these steps when a change touches .NET files (`.cs`, `.csproj`, `.sln`, `.editorconfig`, `.props`, `.targets`). All must
pass.

```bash
dotnet build --configuration Debug --no-restore
dotnet format ZipDir.sln --verify-no-changes
gtimeout 60 dotnet test --no-restore
```

On a format failure, run `dotnet format ZipDir.sln` and rerun the script.

Do not commit with failing tests. If a test fails unexpectedly, investigate before touching the test.

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
dotnet test
```

Unit and integration tests cover:

- ZIP file detection (by extension and magic number)
- Nested ZIP processing
- Pattern matching and exclusion
- Multi-threaded and single-threaded modes
- Raw output formatting
- Configuration validation

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
- LF line endings for all text files, including CS files (enforced by `.gitattributes`). Windows scripts (`.cmd`, `.bat`, `.ps1`) use CRLF.
- Braces on same line for control blocks, new line for methods/types
- Extensive analyzer rules configured in .editorconfig

### Dependencies

- **PicoArgs-dotnet**: Command-line argument parsing
- **Native AOT**: For performance optimization
- **Multiple analyzers**: Roslynator, RecordValueAnalyser, Threading analyzers

## Project Structure

- Single executable project targeting .NET 10.0
- Unit test project (ZipDir.Tests) with unit and integration tests
- Uses record types for configuration with value semantics
- Implements custom equality for collections in records

## Current Package Versions

- Microsoft.DotNet.ILCompiler: 10.0.1
- Microsoft.NET.ILLink.Tasks: 10.0.1
- Roslynator.Analyzers: 4.15.0
- Microsoft.VisualStudio.Threading.Analyzers: 17.14.15
- lookbusy1344.RecordValueAnalyser: 1.2.2
- Microsoft.NET.Test.Sdk: 18.0.1
- xunit: 2.9.3
- coverlet.collector: 6.0.4

## Guidelines

- **IMPORTANT** Every `dotnet` Bash call must set `dangerouslyDisableSandbox: true` (build, test, format, run, restore, publish, and any `gtimeout`
  -wrapped variants). The Claude Code sandbox blocks `dotnet` even when listed in `excludedCommands`: MSBuild's Unix-domain sockets for diagnostic IPC
  and worker-node communication fail under `network-inbound` deny, and the EPERM surfaces as a silent generic build failure.
