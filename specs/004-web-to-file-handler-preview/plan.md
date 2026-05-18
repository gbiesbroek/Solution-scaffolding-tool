# Implementation Plan: WebToFileHandler and Handler Preview Strings

**Branch**: `004-web-to-file-handler-preview` | **Date**: 2026-05-18 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/004-web-to-file-handler-preview/spec.md`

## Summary

Extends feature 003's Root submenu infrastructure with three related additions: (1) a `WebToFileHandler` that downloads a URL to a local file; (2) a `Preview` property on `IScaffoldHandler` shown in a new post-selection confirmation step inside `RootMenuAction`; (3) an overwrite protection flow (shared `IFileSystem` abstraction) applied before any handler writes a file. Introduces `IFileSystem`/`RealFileSystem`, `IHttpFileDownloader`/`HttpFileDownloader`, and `WebToFileHandler` as the only new production types. Modifies `IScaffoldHandler`, `DotnetNewHandler`, `RootMenuAction`, `RootItemRegistry`, and DI registration. No new NuGet packages; no new projects.

## Technical Context

**Language/Version**: C# / .NET 10.0  
**Primary Dependencies**: `Spectre.Console` (existing), `Microsoft.Extensions.DependencyInjection` (existing), `Spectre.Console.Testing` (existing, test-only), `System.Net.Http.HttpClient` (built-in BCL)  
**Storage**: Local file system (write only — `File.WriteAllText` via `IFileSystem`)  
**Testing**: xunit v3 + Microsoft.Testing.Platform — unit tests via `TestConsole`, `StubFileSystem`, `StubHttpFileDownloader`, `StubCommandRunner`; E2E via process launch  
**Target Platform**: Interactive terminal (Windows / Linux / macOS)  
**Project Type**: .NET global tool (CLI) — extending feature 003  
**Performance Goals**: HTTP download completes within 10-second timeout; handler overhead < 500 ms above actual I/O  
**Constraints**: No new NuGet packages; no new test projects; no new source projects  
**Scale/Scope**: Single-developer tool; no concurrency

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Spec-First | Pass | Spec complete with 3 clarifications recorded |
| II. TDD (NON-NEGOTIABLE) | Pass | `IFileSystem`, `IHttpFileDownloader`, `ICommandRunner` all enable unit testing without real I/O; TDD gates enforced in tasks |
| III. Incremental & Independent Delivery | Pass | US1 (WebToFileHandler), US2 (Preview confirmation), US3 (overwrite protection) are independently testable |
| IV. Simplicity First (YAGNI) | Pass | `IFileSystem` has exactly 2 methods; `IHttpFileDownloader` has 1 method; no new packages; overwrite check in handler not framework |
| V. Traceability | Pass | All tasks trace to US1/US2/US3; all decisions in research.md |

**Post-design re-check**: No complexity violations. `IFileSystem` and `IHttpFileDownloader` are the minimum seams for TDD. `RealFileSystem` and `HttpFileDownloader` are trivial wrappers.

## Project Structure

### Documentation (this feature)

```text
specs/004-web-to-file-handler-preview/
├── plan.md                                    # This file
├── research.md                                # Phase 0 decisions (6 decisions)
├── data-model.md                              # Phase 1 entities & state machine
├── quickstart.md                              # Developer usage guide
├── contracts/
│   └── scaffold-web-handler-contract.md       # Updated handler + new type contracts
└── tasks.md                                   # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/Scaffold.Cli/
├── Handlers/
│   ├── IScaffoldHandler.cs        MODIFY — add string Preview { get; }
│   ├── DotnetNewHandler.cs        MODIFY — add IFileSystem + outputFileName params; implement Preview; add overwrite check
│   ├── IFileSystem.cs             NEW
│   ├── RealFileSystem.cs          NEW
│   ├── IHttpFileDownloader.cs     NEW
│   ├── HttpFileDownloader.cs      NEW
│   └── WebToFileHandler.cs        NEW
├── Root/
│   └── RootItemRegistry.cs        MODIFY — inject IFileSystem + IHttpFileDownloader; add WebToFileHandler for .gitignore; add outputFileName to DotnetNewHandlers
├── Menu/
│   └── RootMenuAction.cs          MODIFY — add preview confirmation step in handler execution loop
└── Infrastructure/
    └── ServiceCollectionExtensions.cs  MODIFY — register IFileSystem, HttpClient (10s timeout), IHttpFileDownloader

tests/Scaffold.Cli.Tests/
├── Handlers/
│   ├── StubFileSystem.cs          NEW (test helper)
│   ├── StubHttpFileDownloader.cs  NEW (test helper)
│   ├── DotnetNewHandlerTests.cs   MODIFY — add IFileSystem + outputFileName to all constructor calls; add overwrite tests
│   └── WebToFileHandlerTests.cs   NEW
└── Root/
    ├── RootItemRegistryTests.cs   MODIFY — update constructor call; add WebToFileHandler assertion
    ├── RootMenuActionTests.cs     MODIFY — add preview confirmation + overwrite navigation tests
    └── RootItemExtensibilityTests.cs  MODIFY — update constructor call

tests/Scaffold.Cli.EndToEnd/
└── ScaffoldCliTests.cs            Possibly add E2E assertion for Root → .gitignore flow
```

**Structure Decision**: All new production types go into the existing `src/Scaffold.Cli/Handlers/` folder (alongside `ICommandRunner`, `DotnetNewHandler`, etc.). No new projects or packages required.

