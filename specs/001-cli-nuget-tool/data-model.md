# Data Model: Interactive CLI NuGet Tool

**Feature**: 001-cli-nuget-tool
**Phase**: 1 — Design

---

## Overview

This feature has no persistent data store and no domain entities with mutable state.
The data model documents the structural concepts and their relationships for implementation
clarity.

---

## Entities

### CLI Tool Process

Represents one running instance of the `scaffold` tool.

| Field | Type | Description |
|-------|------|-------------|
| stdout | Stream | Output channel — used to write the prompt message |
| stdin | Stream | Input channel — read once, then process exits |
| exitCode | int | 0 on clean exit after any input is received |

**Lifecycle**:
```
Launched → Writes prompt to stdout → Blocks on stdin.ReadLine() → Receives input → Exits (code 0)
```

**Constraints**:
- The prompt message MUST be written to stdout before stdin is read.
- The tool MUST NOT exit before stdin produces a line (or EOF).
- Exit code MUST be 0 on normal termination.

---

### NuGet Package

Represents the distributable artifact produced by `dotnet pack`.

| Field | Type | Description |
|-------|------|-------------|
| PackageId | string | `Scaffold.Cli` |
| ToolCommandName | string | `scaffold` |
| Version | string | Semantic version, starting at `1.0.0` |
| TargetFramework | string | `net9.0` |
| PackAsTool | bool | `true` — marks this as a .NET global tool |

**Constraints**:
- `PackageId` and `ToolCommandName` are fixed for this version.
- Version MUST follow SemVer; increment for each republish to the feed.

---

### Local NuGet Feed

Represents the local directory acting as a NuGet package source.

| Field | Type | Description |
|-------|------|-------------|
| path | string | Absolute path to `~/nuget-local` in the user's home directory |
| sourceName | string | `scaffold-local` (registered name in user NuGet config) |
| contents | *.nupkg files | Packed tool packages published here |

**Constraints**:
- The directory MUST exist before `dotnet nuget add source` is called.
- The source name `scaffold-local` MUST be unique in the user's NuGet config.
- `.nupkg` files in this directory are not committed to source control.

---

### End-to-End Test

Represents a single automated test case verifying the tool's interactive behavior.

| Field | Type | Description |
|-------|------|-------------|
| processUnderTest | Process | OS process running the `scaffold` binary |
| stdoutCapture | string | Full stdout output read after process start |
| stdinInput | string | The line of text written to stdin by the test |
| exitCode | int | Expected: 0 |
| timeout | TimeSpan | Maximum wait time: 5 seconds (per SC-002) |
| idleCheckDelay | TimeSpan | Time to wait before asserting process is still running: 500ms |

**Constraints**:
- The test MUST assert `stdoutCapture` contains the prompt message before sending input.
- The test MUST assert the process is still running after `idleCheckDelay` (not crashed).
- The test MUST assert `exitCode == 0` after input is sent.

---

## Relationships

```
NuGet Package ──(packed into)──► Local NuGet Feed
Local NuGet Feed ──(source for)──► dotnet tool install
dotnet tool install ──(produces)──► CLI Tool Binary
CLI Tool Binary ──(launched by)──► End-to-End Test
End-to-End Test ──(sends input to)──► CLI Tool Process
```

---

## State Transitions

### CLI Tool Process States

```
[Not Started]
     │
     ▼ (launched)
[Writing Prompt]
     │
     ▼ (stdout written)
[Waiting for Input]  ◄── test asserts: process is alive here
     │
     ▼ (stdin line received or EOF)
[Exiting]
     │
     ▼ (exit code 0)
[Terminated]         ◄── test asserts: exitCode == 0
```
