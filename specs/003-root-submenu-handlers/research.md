# Research: Root Submenu with Command Handlers

**Feature**: 003-root-submenu-handlers | **Date**: 2026-05-18

---

## Decision 1: IScaffoldHandler Interface Design

**Decision**: Single `ExecuteAsync(IAnsiConsole console, HandlerContext context)` method plus a `DisplayName` string property.

**Rationale**: Mirrors the existing `IMenuAction` pattern exactly, making the codebase consistent. `DisplayName` is needed for the handler selection menu. The single method keeps the abstraction minimal — each handler decides internally whether to run a command, prompt for input, or do both.

**Alternatives considered**:
- Separate `ICommandHandler` and `IInputHandler` sub-interfaces: rejected (YAGNI — no concrete need to distinguish at runtime yet; the split can always be introduced later without breaking the base interface)
- Generic `IScaffoldHandler<TConfig>`: rejected (over-engineering; config is baked in at construction time via constructor args)

---

## Decision 2: RootMenuAction Navigation Loop

**Decision**: `RootMenuAction` implements `IMenuAction` and owns a two-level loop internally: (1) Root item selection, (2) handler selection for the chosen item. Uses `SelectionPrompt<string>` with `UseConverter` pattern for both levels. After handler completes, returns to level 1 (Root item menu). "<- Back" at level 1 calls `navContext.GoBack()`.

**Rationale**: Keeps all Root-specific navigation self-contained in one class — `AppRunner` does not need to know about Root's two-level structure. Consistent with how `StubMenuAction` currently owns its own sub-prompt.

**Alternatives considered**:
- Separate `RootMenuRenderer`: rejected (YAGNI — the `IMenuRenderer` is typed to `Category`; a parallel renderer for `RootItem` would add a class purely for indirection; direct use of `IAnsiConsole.Prompt` inside the action is simpler)
- Letting `AppRunner` drive the Root submenu: rejected (breaks single-responsibility; AppRunner would need Root-specific knowledge)

---

## Decision 3: ICommandRunner Abstraction

**Decision**: Extract `ICommandRunner` interface with a single `RunAsync(string executable, IEnumerable<string> args, string workingDirectory)` → `CommandResult` method. Provide `ProcessCommandRunner` as the production implementation. Register as singleton in DI.

**Rationale**: Required for TDD — `DotnetNewHandler` must be unit-testable without actually running `dotnet new`. `ICommandRunner` is a minimal seam: it is the only point where real OS process execution happens. `ProcessCommandRunner` is not unit-tested directly (trivial wrapper around `System.Diagnostics.Process`); it is covered by E2E tests.

**Alternatives considered**:
- Testing `DotnetNewHandler` with real `dotnet new` execution: rejected (slow, side-effects in working directory, non-deterministic in CI)
- Mocking `Process` directly: rejected (sealed class, not mockable without adapters)

---

## Decision 4: RootItem Handler Registration Pattern

**Decision**: `RootItemRegistry` constructor takes `ICommandRunner` and `new`s `DotnetNewHandler` instances inline with configured template names. Each `RootItem` holds `IReadOnlyList<IScaffoldHandler>`. `CategoryRegistry` takes `RootMenuAction` as a constructor parameter to replace `StubMenuAction("Root")`.

**Rationale**: Mirrors `CategoryRegistry` construction pattern. `DotnetNewHandler` instances are value-like objects (stateless, configured at construction) — constructing them in the registry is appropriate. `CategoryRegistry` receives `RootMenuAction` via DI, keeping it decoupled from `IRootItemRegistry`.

**Alternatives considered**:
- Registering each `DotnetNewHandler` in DI: rejected (multiple instances of same type with different config requires named registrations, which is complex in `Microsoft.Extensions.DependencyInjection`)
- Hard-coding handlers inside `RootMenuAction`: rejected (violates single responsibility — registry owns item definitions)

---

## Decision 5: HandlerContext — Standalone Type

**Decision**: `HandlerContext` is a new standalone class with `ShouldGoBack` (`bool`, read-only), `GoBack()` (`void`), and `WorkingDirectory` (`string`, set at construction). No reference to `NavigationContext`.

**Rationale**: Confirmed by clarification (Q2 answer). Handler layer has different concerns from navigation layer. `WorkingDirectory` is meaningful to handlers (file creation target) but irrelevant to top-level navigation. Clean separation prevents coupling of navigation signals across layers.

**Alternatives considered**:
- Reuse `NavigationContext`: rejected per clarification
- `HandlerContext` wraps `NavigationContext`: rejected per clarification
