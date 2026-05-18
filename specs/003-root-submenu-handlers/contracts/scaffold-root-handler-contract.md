# Contract: Root Submenu and Scaffold Handlers

**Feature**: 003-root-submenu-handlers | **Date**: 2026-05-18

---

## Root Navigation Contract

Selecting "Root" from the top-level scaffold menu MUST display a Root item submenu:

```
? Root scaffolding
> .gitignore
  .gitattributes
  <- Back
```

Selecting a Root item MUST display a handler selection menu:

```
? How would you like to scaffold .gitignore?
> dotnet new
  <- Back
```

- `"<- Back"` at the Root item menu returns to the top-level menu
- `"<- Back"` at the handler selection menu returns to the Root item menu
- After a handler completes, the Root item menu is re-displayed

---

## IScaffoldHandler Contract

```csharp
public interface IScaffoldHandler
{
    string DisplayName { get; }
    Task ExecuteAsync(IAnsiConsole console, HandlerContext context);
}
```

Any implementation MUST:
1. Expose a non-empty `DisplayName` (shown in the handler selection menu).
2. Use only the injected `IAnsiConsole` for all terminal output — no static `AnsiConsole.*` calls.
3. Call `context.GoBack()` exactly once before returning, regardless of success or failure.
4. NOT call `Environment.Exit()` directly.

---

## HandlerContext Contract

```csharp
public class HandlerContext
{
    public string WorkingDirectory { get; }
    public bool ShouldGoBack { get; private set; }

    public HandlerContext(string workingDirectory);
    public void GoBack();
}
```

- `WorkingDirectory` is set to `Directory.GetCurrentDirectory()` at invocation time.
- `ShouldGoBack` starts `false`; `GoBack()` sets it to `true`.
- No dependency on `NavigationContext`.

---

## ICommandRunner Contract

```csharp
public interface ICommandRunner
{
    Task<CommandResult> RunAsync(string executable, IEnumerable<string> args, string workingDirectory);
}
```

Any implementation MUST:
1. Pass `args` individually (not shell-concatenated) to prevent injection.
2. Capture both stdout and stderr without truncation.
3. Await process completion before returning.
4. Return the actual exit code (not normalise to 0/1).

---

## DotnetNewHandler Contract

- `DisplayName` returns `"dotnet new"`.
- Constructed with `(string templateName, ICommandRunner commandRunner, IReadOnlyList<string>? extraArgs = null)`.
- `ExecuteAsync` runs: `dotnet new <templateName> [extraArgs...]` in `context.WorkingDirectory`.
- Displays `Output` and `Error` from `CommandResult` via `IAnsiConsole`.
- Always calls `context.GoBack()` on completion.

---

## IRootItemRegistry Contract

```csharp
public interface IRootItemRegistry
{
    IReadOnlyList<RootItem> GetItems();
}
```

Any implementation MUST:
1. Return a non-empty, ordered list.
2. Each `RootItem` must have at least one `IScaffoldHandler`.
3. Return the same list on every call (stable).

---

## Testability Contract

- All production classes that render menus MUST accept `IAnsiConsole` via constructor or method parameter.
- `DotnetNewHandler` MUST accept `ICommandRunner` via constructor (enables mock in tests).
- `RootMenuAction` MUST accept `IRootItemRegistry` via constructor.
- No class may call `AnsiConsole.*` static methods directly.
- Tests MUST use `TestConsole` from `Spectre.Console.Testing` for keyboard simulation.
- Tests for `DotnetNewHandler` MUST use a mock/stub `ICommandRunner` — no real process invocation in unit tests.
