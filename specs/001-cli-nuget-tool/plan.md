# Implementation Plan: Interactive CLI NuGet Tool (`scaffold`)

**Branch**: `001-cli-nuget-tool` | **Date**: 2026-05-18 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/001-cli-nuget-tool/spec.md`

## Summary

Build a C# .NET 10 global tool named `scaffold` that runs interactively: it prints a
prompt to stdout, waits for any line of input from stdin, then exits cleanly (code 0).
The tool is packed into a local NuGet feed directory (`~/nuget-local` in the user's home
directory) and installed via `dotnet tool install --global Scaffold.Cli`. An xunit v3 +
Microsoft.Testing.Platform E2E test project under `/tests` verifies the interactive behavior
by launching the tool binary as a subprocess and asserting stdout output, process liveness,
and exit code.

## Technical Context

**Language/Version**: C# / .NET 9.0
**Primary Dependencies**: xunit v2, Microsoft.Testing.Platform (MTP) v1, xunit.runner.visualstudio v2, Microsoft.NET.Test.Sdk v17
**Storage**: N/A
**Testing**: xunit v2 + Microsoft.Testing.Platform via `dotnet test`
**Target Platform**: Cross-platform developer workstation (Windows / Linux / macOS)
**Project Type**: .NET global tool (CLI)
**Performance Goals**: Prompt appears < 1 second after launch; exit within 5 seconds of receiving input
**Constraints**: Interactive mode only — no CLI arguments, no configuration, no persistence
**Scale/Scope**: Single-developer tool; no concurrency requirements

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Spec-First | ✅ Pass | Spec is complete and clarified before this plan |
| II. TDD (NON-NEGOTIABLE) | ✅ Pass | E2E tests planned (US2); tests written before implementation |
| III. Incremental & Independent Delivery | ✅ Pass | US1 (tool + publish) and US2 (E2E tests) are independent |
| IV. Simplicity First (YAGNI) | ✅ Pass | Simplest possible stdin/stdout pattern; no frameworks |
| V. Traceability | ✅ Pass | All tasks trace to US1 or US2; decisions in research.md |

**Post-design re-check**: All gates still pass. No complexity violations.

## Project Structure

### Documentation (this feature)

```text
specs/001-cli-nuget-tool/
├── plan.md                          # This file
├── research.md                      # Phase 0 decisions
├── data-model.md                    # Phase 1 entities & state machine
├── quickstart.md                    # Setup & usage guide
├── contracts/
│   └── scaffold-cli-contract.md    # stdin/stdout/exit code contract
└── tasks.md                        # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
└── Scaffold.Cli/
    ├── Scaffold.Cli.csproj
    └── Program.cs

tests/
└── Scaffold.Cli.EndToEnd/
    ├── Scaffold.Cli.EndToEnd.csproj
    └── ScaffoldCliTests.cs

~/nuget-local/             # Local NuGet feed (home dir, outside repo)
SolutionScaffoldingTool.sln
```

**Structure Decision**: Single-project layout. Source under `src/Scaffold.Cli/`,
end-to-end tests under `tests/Scaffold.Cli.EndToEnd/`. No unit or integration test
projects needed — the tool logic is trivial (one `Console.ReadLine()` call) and fully
covered by the E2E test.

## Complexity Tracking

> No constitution violations. Table intentionally empty.

