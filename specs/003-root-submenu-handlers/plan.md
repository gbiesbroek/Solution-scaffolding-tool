# Implementation Plan: Root Submenu with Command Handlers

**Branch**: `003-root-submenu-handlers` | **Date**: 2026-05-18 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/003-root-submenu-handlers/spec.md`

## Summary

Replace `StubMenuAction("Root")` in `CategoryRegistry` with a real `RootMenuAction` that renders a
two-level interactive submenu: Root item selection then handler selection then command execution.
Introduces `IScaffoldHandler`, `HandlerContext`, `ICommandRunner`, `DotnetNewHandler`, `RootItem`,
and `IRootItemRegistry` as the extensible scaffolding infrastructure. Uses the existing
`Microsoft.Extensions.DependencyInjection` + `Spectre.Console` + `Spectre.Console.Testing` stack.
No new projects or packages required.

## Technical Context

**Language/Version**: C# / .NET 10.0
**Primary Dependencies**: `Spectre.Console` (existing), `Microsoft.Extensions.DependencyInjection` (existing), `Spectre.Console.Testing` (existing, test-only)
**Storage**: N/A
**Testing**: xunit v3 + Microsoft.Testing.Platform - unit tests via `TestConsole` + stub `ICommandRunner`; E2E via process launch
**Target Platform**: Interactive terminal (Windows / Linux / macOS)
**Project Type**: .NET global tool (CLI) - extending feature 002
**Performance Goals**: Root submenu renders in < 1 second; handler command overhead < 500 ms above actual command duration
**Constraints**: No new NuGet packages; no new test projects; no new source projects
**Scale/Scope**: Single-developer tool; no concurrency

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Spec-First | Pass | Spec complete with 2 clarifications recorded |
| II. TDD (NON-NEGOTIABLE) | Pass | `ICommandRunner` enables unit testing of `DotnetNewHandler`; `HandlerContext` testable as pure value object; TDD gates enforced in tasks |
| III. Incremental & Independent Delivery | Pass | US1 (Root submenu navigation), US2 (handler execution), US3 (extensibility) are independent |
| IV. Simplicity First (YAGNI) | Pass | No new packages; no new projects; `RootMenuAction` owns its loop directly; `ICommandRunner` is the only new seam |
| V. Traceability | Pass | All tasks trace to US1/US2/US3; all decisions in research.md |

**Post-design re-check**: No complexity violations. `ICommandRunner` is the minimum abstraction required for TDD on `DotnetNewHandler`. All other types are direct implementations.

## Project Structure

### Documentation (this feature)

```
specs/003-root-submenu-handlers/
├── plan.md                                # This file
├── research.md                            # Phase 0 decisions
├── data-model.md                          # Phase 1 entities & state machine
├── quickstart.md                          # Developer usage guide
├── contracts/
│   └── scaffold-root-handler-contract.md  # Interface and navigation contracts
└── tasks.md                               # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```
src/Scaffold.Cli/
├── Program.cs                             # Unchanged
├── AppRunner.cs                           # Unchanged
├── Infrastructure/
│   └── ServiceCollectionExtensions.cs    # Updated: add ICommandRunner, IRootItemRegistry, RootMenuAction
├── Menu/                                  # Unchanged
├── Categories/
│   └── CategoryRegistry.cs              # Updated: Root uses RootMenuAction instead of StubMenuAction
├── Actions/                               # Unchanged (StubMenuAction still used for Aspire/Frontend/Core)
├── Root/
│   ├── RootItem.cs                       # NEW
│   ├── IRootItemRegistry.cs              # NEW
│   ├── RootItemRegistry.cs              # NEW
│   └── RootMenuAction.cs                 # NEW
└── Handlers/
    ├── IScaffoldHandler.cs               # NEW
    ├── HandlerContext.cs                 # NEW
    ├── ICommandRunner.cs                 # NEW
    ├── CommandResult.cs                  # NEW
    ├── ProcessCommandRunner.cs           # NEW
    └── DotnetNewHandler.cs               # NEW

tests/Scaffold.Cli.Tests/
├── Root/
│   ├── RootItemRegistryTests.cs          # NEW
│   └── RootMenuActionTests.cs            # NEW
└── Handlers/
    ├── HandlerContextTests.cs            # NEW
    └── DotnetNewHandlerTests.cs          # NEW (uses StubCommandRunner)

tests/Scaffold.Cli.EndToEnd/
└── ScaffoldCliTests.cs                   # May add Root navigation assertion
```

**Structure Decision**: New code under `Root/` (navigation + registry) and `Handlers/` (execution) inside the existing `src/Scaffold.Cli/` project. No new projects or packages required.

## Complexity Tracking

| Abstraction | Why Needed |
|-------------|------------|
| `ICommandRunner` | TDD requires `DotnetNewHandler` to be unit-testable without running `dotnet new` (side effects, slow, non-deterministic) |
