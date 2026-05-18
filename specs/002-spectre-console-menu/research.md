# Research: Spectre.Console Interactive Category Menu

**Feature**: 002-spectre-console-menu | **Date**: 2026-05-18

---

## Decision 1: Spectre.Console API for Interactive Menu

**Decision**: Use `SelectionPrompt<Category>` with a `.UseConverter(c => c.DisplayName)` to render
each `Category` object while returning the typed entity. Call it via injected `IAnsiConsole.Prompt(...)`.

**Rationale**: `SelectionPrompt<T>` supports generic types natively — it renders via a converter
string but returns the actual `T` instance. This means the menu can be typed to `Category` without
any string-to-object lookup after selection. The `.WrapAround()` option handles boundary wrap
natively. `IAnsiConsole.Prompt(prompt)` is the injectable extension method; the static
`AnsiConsole.Prompt(...)` delegates to it, so no static calls are needed anywhere.

**Alternatives considered**:
- `Spectre.Console.Cli` (`CommandApp` + `Command<TSettings>`) — rejected because the spec
  calls for a menu-driven interactive loop, not a command-line argument parser. `CommandApp`
  is designed for `git`-style subcommand dispatch, not arrow-key menus.
- `TextPrompt<string>` — rejected; requires the user to type text rather than navigate with
  arrow keys.

---

## Decision 2: Dependency Injection Strategy

**Decision**: Use `Microsoft.Extensions.DependencyInjection` directly (no `IHost`, no
`WebApplication`). `ServiceCollection` → `BuildServiceProvider()` → `GetRequiredService<T>()`.
Register `IAnsiConsole` as `services.AddSingleton<IAnsiConsole>(AnsiConsole.Console)`.

**Rationale**: This is the lightest-weight DI approach for a console tool. `IHost` adds
`Microsoft.Extensions.Hosting` and its logging/configuration infrastructure — unnecessary overhead
for a simple interactive menu. The plain `ServiceCollection` pattern is well-established, used in
production tools (e.g., `BookGen`), and keeps the `Program.cs` minimal. `BuildServiceProvider()`
is wrapped in `using` to ensure singleton disposal.

**Alternatives considered**:
- `IHost` / `HostApplicationBuilder` — rejected (YAGNI; no background services, no hosted
  lifecycle, no config system needed).
- Static `AnsiConsole` calls — rejected; not testable. `IAnsiConsole` injection is the
  Spectre.Console-recommended testability pattern.
- `Spectre.Console.Cli` `ITypeRegistrar` bridge — rejected for the same reason as Decision 1;
  `CommandApp` is not appropriate here.

---

## Decision 3: Testing Strategy — Unit Tests with TestConsole

**Decision**: Add a new unit test project `tests/Scaffold.Cli.Tests/` using xunit v3 + MTP +
`Spectre.Console.Testing`. All menu logic is tested there using `new TestConsole()` in place of
the real console. The existing E2E project is updated to verify only the process-level contract
(the app starts, renders menu text, and exits cleanly when "Exit" is selected via stdin).

**Rationale**: `Spectre.Console.Testing.TestConsole` implements `IAnsiConsole` and captures all
output in `TestConsole.Output`. It supports keyboard simulation via `console.Input.PushKey(ConsoleKey.DownArrow)`.
This is the authoritative Spectre.Console test approach. The existing E2E tests launch a real process
and pipe stdin — this is incompatible with Spectre.Console's raw terminal keyboard input for
`SelectionPrompt`. Therefore two test layers are needed:
1. **Unit** (`Scaffold.Cli.Tests`): menu rendering, navigation, action dispatch — fast, no process.
2. **E2E** (`Scaffold.Cli.EndToEnd`): process launches, menu text visible in stdout, Exit terminates.

**Alternatives considered**:
- Extending only the E2E project — rejected; E2E stdin piping cannot drive `SelectionPrompt`
  interactive keyboard navigation.
- Single test project — rejected; mixing process-level tests with unit tests creates slow,
  fragile test runs.

---

## Decision 4: IMenuAction Abstraction

**Decision**: Define `IMenuAction` with a single `Task ExecuteAsync(IAnsiConsole console, NavigationContext context)` method.
`ExitMenuAction` sets `context.Exit()`. Stub actions for Aspire/Frontend/Core/Root display a
placeholder message and call `context.GoBack()` (immediately returning). `NavigationContext`
is a simple mutable class carrying `ShouldExit` and `ShouldGoBack` flags set by calling
`Exit()` / `GoBack()`.

**Rationale**: Single-method interface keeps the abstraction minimal (YAGNI). Passing both
`IAnsiConsole` and `NavigationContext` into `Execute` gives each action full rendering capability
without needing its own DI-resolved console reference, while still keeping actions testable
(inject a `TestConsole` in tests). Future sub-menu actions can build their own `SelectionPrompt`
using the passed console.

**Alternatives considered**:
- `NavigationContext` as DI-registered singleton — rejected; context is per-navigation-frame,
  not app-global. Creating it per invocation avoids state contamination across loops.
- Returning a `NavigationResult` enum from `Execute` instead of mutating context — considered;
  rejected to keep the interface simpler and allow future context properties without changing the return type.

---

## Decision 5: Category Registry — Single Location

**Decision**: A `CategoryRegistry` class (registered as `ICategoryRegistry` singleton) builds and
returns the ordered list of `Category` objects. The list is constructed inline in the registry's
constructor or factory method. Adding a new category requires only editing `CategoryRegistry`.

**Rationale**: FR-009 mandates a single location for category definitions. A dedicated registry
class satisfies this cleanly. The registry is tested independently; the menu renderer depends on
`ICategoryRegistry` — it never hardcodes any category names.

**Alternatives considered**:
- JSON/YAML config file for categories — rejected (YAGNI; categories are developer-defined, not
  user-configurable, and this would add a file-parsing dependency).
- Hardcoding categories directly in `Program.cs` — rejected; violates the single-location and
  extensibility requirements.

---

## Key Package Versions

| Package | Version | Used in |
|---------|---------|---------|
| `Spectre.Console` | latest stable (≥0.49) | `src/Scaffold.Cli` |
| `Microsoft.Extensions.DependencyInjection` | 10.x | `src/Scaffold.Cli` |
| `Spectre.Console.Testing` | latest stable (≥0.49) | `tests/Scaffold.Cli.Tests` |
| `xunit.v3.mtp-v2` | 3.2.2 | `tests/Scaffold.Cli.Tests` |

---

## Summary Table

| Decision | Choice | Key Reason |
|---|---|---|
| Menu API | `SelectionPrompt<Category>` via `IAnsiConsole` | Injectable, typed result |
| DI | `ServiceCollection` + `BuildServiceProvider` | No IHost overhead (YAGNI) |
| Testing | Separate unit project + `TestConsole` | E2E stdin can't drive SelectionPrompt |
| Action abstraction | `IMenuAction.ExecuteAsync(console, context)` | Minimal, testable, extensible |
| Category location | `CategoryRegistry` singleton | FR-009 single-location requirement |
