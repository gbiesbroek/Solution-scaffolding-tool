# Implementation Plan: Spectre.Console Interactive Category Menu

**Branch**: `002-spectre-console-menu` | **Date**: 2026-05-18 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/002-spectre-console-menu/spec.md`

## Summary

Replace the current plain `Console.ReadLine()` prompt in the `scaffold` CLI tool with an
interactive Spectre.Console `SelectionPrompt<Category>` menu showing five top-level categories:
Aspire, Frontend, Core, Root, and Exit. The tool uses `Microsoft.Extensions.DependencyInjection`
(no IHost) with `IAnsiConsole` injected throughout for testability. Menu logic is unit-tested
via `Spectre.Console.Testing.TestConsole`; process-level behaviour is covered by the existing
E2E test project. The design uses `IMenuAction` + `NavigationContext` + `ICategoryRegistry`
abstractions to support unlimited future extension without modifying the menu infrastructure.

## Technical Context

**Language/Version**: C# / .NET 10.0
**Primary Dependencies**: `Spectre.Console` (>=0.49), `Microsoft.Extensions.DependencyInjection` (10.x), `Spectre.Console.Testing` (>=0.49, test-only)
**Storage**: N/A
**Testing**: xunit v3 + Microsoft.Testing.Platform -- unit tests via `TestConsole`; E2E via process launch
**Target Platform**: Interactive terminal (Windows / Linux / macOS developer workstation)
**Project Type**: .NET global tool (CLI)
**Performance Goals**: Menu renders in < 1 second; Exit completes in < 500 ms
**Constraints**: Interactive terminal required; no CLI arguments parsed; no persistence
**Scale/Scope**: Single-developer tool; no concurrency

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Spec-First | Pass | Spec complete and clarified (2 clarifications recorded in spec.md) |
| II. TDD (NON-NEGOTIABLE) | Pass | Unit tests (TestConsole) written before implementation; E2E tests updated |
| III. Incremental & Independent Delivery | Pass | US1 (menu + Exit), US2 (back navigation), US3 (extensibility) are independent |
| IV. Simplicity First (YAGNI) | Pass | No IHost, no Spectre.Console.Cli -- plain ServiceCollection + SelectionPrompt only |
| V. Traceability | Pass | All tasks trace to US1/US2/US3; decisions in research.md |

**Post-design re-check**: New unit test project justified by concrete incompatibility between
E2E stdin piping and Spectre.Console raw terminal keyboard input (see research.md Decision 3).
Recorded in Complexity Tracking below.

## Project Structure

### Documentation (this feature)

```
specs/002-spectre-console-menu/
├── plan.md                              # This file
├── research.md                          # Phase 0 decisions
├── data-model.md                        # Phase 1 entities & state machine
├── quickstart.md                        # Developer setup & usage guide
├── contracts/
│   └── scaffold-menu-contract.md        # CLI, IMenuAction, ICategoryRegistry contracts
└── tasks.md                             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```
src/Scaffold.Cli/
├── Program.cs                             # DI setup + main navigation loop
├── Infrastructure/
│   └── ServiceCollectionExtensions.cs    # AddScaffoldServices() extension method
├── Menu/
│   ├── IMenuRenderer.cs
│   ├── MenuRenderer.cs                   # SelectionPrompt<Category> via IAnsiConsole
│   └── NavigationContext.cs              # Per-frame navigation signal (Exit / GoBack)
├── Categories/
│   ├── Category.cs                       # DisplayName + IMenuAction
│   ├── ICategoryRegistry.cs
│   └── CategoryRegistry.cs              # SINGLE place to define categories
└── Actions/
    ├── IMenuAction.cs                    # Task ExecuteAsync(IAnsiConsole, NavigationContext)
    ├── ExitMenuAction.cs                 # context.Exit()
    └── StubMenuAction.cs                 # placeholder for Aspire/Frontend/Core/Root

tests/Scaffold.Cli.Tests/                 # NEW -- unit tests via TestConsole
├── Scaffold.Cli.Tests.csproj
├── Menu/
│   ├── MenuRendererTests.cs
│   └── NavigationContextTests.cs
├── Actions/
│   └── ExitMenuActionTests.cs
└── Categories/
    └── CategoryRegistryTests.cs

tests/Scaffold.Cli.EndToEnd/              # EXISTING -- updated for new menu output
└── ScaffoldCliTests.cs                   # Updated: assert menu title visible in stdout
```

**Structure Decision**: `Scaffold.Cli.Tests` unit test project added alongside the existing E2E
project. Justified by concrete incompatibility (see Complexity Tracking). All source under
`src/Scaffold.Cli/` organised by concern (Menu / Categories / Actions / Infrastructure).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| Second test project (Scaffold.Cli.Tests) | SelectionPrompt uses raw terminal I/O, incompatible with E2E stdin pipe | Single project cannot drive keyboard navigation -- E2E tests would skip all menu interaction coverage |
