# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

mrGitTags is a CLI tool for managing git tags in mono-repos that publish multiple packages. It tracks version tags per project using the format `{project_name}_{semver}` and helps determine when packages need publishing by analyzing changes since the last tag.

Built with CommandDotNet framework as a dog-fooding exercise. The tool was created for the [CommandDotNet](https://github.com/bilal-fazlani/commanddotnet/) repository.

## Build and Run Commands

**Build the project:**
```bash
dotnet build
```

**Run the tool:**
```bash
dotnet run -- <command> [options]
```

**Run status command (most common):**
```bash
dotnet run -- status
```

**List projects:**
```bash
dotnet run -- projects
```

**Show help:**
```bash
dotnet run -- --help
dotnet run -- <command> --help
```

## Key Commands

The tool provides four main commands:

- **`projects`** - Lists all publishable projects in the repository
- **`tags`** - Lists tags for specified projects with commit history
- **`status`** - Shows status of each project since last tag (most commonly used)
- **`increment`** - Creates and optionally pushes a new tag for a project

## Architecture

### Core Concepts

**Tag Format:** Tags follow `{ProjectName}_{SemVer}` pattern (e.g., `CommandDotNet_3.5.0`)

**Project Discovery:** Scans for `*.*proj` files recursively, filtering out test/example projects (ending with Test, Tests, Example, Examples)

**Mono-repo Structure:** Assumes multiple projects in subdirectories, each with its own versioning

### Key Classes and Responsibilities

**`App.cs`** - CommandDotNet command definitions
- `Intercept()` - Sets up shared infrastructure (repo, console, writer)
- `projects()` - Lists all publishable projects
- `tags()` - Displays tag history with commits/files
- `status()` - Shows changes since last tag per project
- `increment()` - Creates new version tags

**`Repo.cs`** - Repository-level operations
- Wraps LibGit2Sharp.Repository
- Discovers projects by finding `.csproj/.fsproj` files
- Parses and organizes tags by project name
- Links tags in chronological order (Previous/Next)

**`Project.cs`** - Project-level operations
- Represents a single publishable project
- Tracks tags for this project only
- Calculates file/commit changes between tags
- Creates new version tags via `Increment()`

**`TagInfo.cs`** - Tag parsing and metadata
- Parses `{ProjectName}_{SemVer}` format
- Links to Previous/Next tags for navigation
- Wraps LibGit2Sharp.Tag with project context

**`Program.cs`** - CommandDotNet configuration
- Sets up middleware pipeline (Spectre, prompting, name casing)
- Configures SemVersion type descriptor
- Error handling

### Configuration

**`.mrGitTags/config.json`** - Persists skipped commits per project
- Used by `status -i` interactive mode
- Stores SHA of commits to ignore when checking for changes

### Dependencies

- **CommandDotNet** - CLI framework with rich argument parsing
- **CommandDotNet.Spectre** - Spectre.Console integration for rich output
- **LibGit2Sharp** - Git operations
- **Semver** - Semantic versioning
- **Pastel** - Terminal color output
- **MoreLINQ** - LINQ extensions
- **CliWrap** - Executes git CLI commands for push operations

### Execution Flow

1. `Program.Main()` builds AppRunner with CommandDotNet configuration
2. `App.Intercept()` initializes Repo and shared infrastructure
3. Command methods execute (e.g., `status()`, `increment()`)
4. `Repo` discovers projects and parses tags
5. `Project` calculates changes using LibGit2Sharp diffing
6. Output formatted with Pastel colors and Spectre.Console

### Important Patterns

**Project Selection:** Commands accept project identifiers as:
- Project name (e.g., "CommandDotNet")
- Index number (e.g., "0" or "#0")

**Change Detection:** Uses LibGit2Sharp `TreeChanges` to diff commits between tags and branch tip

**Interactive Mode:** `status -i` prompts to increment versions for projects with changes, with options to skip or ignore future changes

**Git Integration:** Generates git comparison URLs and command suggestions for manual verification
