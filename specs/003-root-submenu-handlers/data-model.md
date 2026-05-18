# Data Model: Root Submenu with Command Handlers

**Feature**: 003-root-submenu-handlers | **Date**: 2026-05-18

---

## Entities

### RootItem

A named scaffold item displayed in the Root submenu. Holds an ordered list of one or more handlers. When selected by the user, a handler selection menu is shown.

| Field | Type | Notes |
|---|---|---|
| `DisplayName` | `string` | Shown in the Root submenu. Required, non-empty. |
| `Handlers` | `IReadOnlyList<IScaffoldHandler>` | At least one handler required. Displayed in handler selection menu. |

---

### IRootItemRegistry / RootItemRegistry

Single authoritative source for the ordered list of Root submenu items.

| Member | Signature | Notes |
|---|---|---|
| `GetItems()` | `IReadOnlyList<RootItem>` | Returns all root items in display order. Stable, deterministic. |

Initial list (in order):
1. `.gitignore` → `[DotnetNewHandler("gitignore")]`
2. `.gitattributes` → `[DotnetNewHandler("gitattributes")]`

Adding a new root item requires editing `RootItemRegistry` only.

---

### IScaffoldHandler

Abstraction for the action executed when a handler is selected from the handler selection menu.

| Member | Type / Signature | Notes |
|---|---|---|
| `DisplayName` | `string` | Shown in the handler selection menu. Required, non-empty. |
| `ExecuteAsync` | `Task ExecuteAsync(IAnsiConsole console, HandlerContext context)` | Executes the handler's action. Must signal `context.GoBack()` when done. |

**Known implementations:**

| Class | DisplayName | Behaviour |
|---|---|---|
| `DotnetNewHandler` | `"dotnet new"` | Runs `dotnet new <templateName> [extraArgs...]` via `ICommandRunner`. Displays output. Calls `context.GoBack()`. |

---

### HandlerContext

Standalone per-invocation state passed to every `IScaffoldHandler.ExecuteAsync` call.

| Field / Method | Type | Notes |
|---|---|---|
| `WorkingDirectory` | `string` (read-only) | Set at construction to the process working directory at launch. |
| `ShouldGoBack` | `bool` (read-only) | Set to `true` by calling `GoBack()`. Signals the handler to return. |
| `GoBack()` | `void` | Sets `ShouldGoBack = true`. |

A new `HandlerContext` instance is created for each `ExecuteAsync` invocation. Has no dependency on `NavigationContext`.

---

### DotnetNewHandler

Concrete `IScaffoldHandler` that executes `dotnet new <templateName> [extraArgs...]`.

| Field | Type | Notes |
|---|---|---|
| `_templateName` | `string` | Configured at construction. E.g. `"gitignore"`, `"gitattributes"`. |
| `_extraArgs` | `IReadOnlyList<string>` | Optional extra arguments. Default empty. Appended after template name. |
| `_commandRunner` | `ICommandRunner` | Injected. Executes the process. |
| `DisplayName` | `string` | Returns `"dotnet new"`. |

---

### ICommandRunner / ProcessCommandRunner

Abstraction over OS process execution. Enables unit testing of `DotnetNewHandler`.

| Member | Signature | Notes |
|---|---|---|
| `RunAsync` | `Task<CommandResult> RunAsync(string executable, IEnumerable<string> args, string workingDirectory)` | Starts the process, waits for exit, captures stdout + stderr. |

`ProcessCommandRunner` is the production implementation. Uses `System.Diagnostics.Process` with `ArgumentList` (safe arg passing, no shell injection) and `WorkingDirectory`.

---

### CommandResult

Captures the outcome of a command execution.

| Field | Type | Notes |
|---|---|---|
| `ExitCode` | `int` | 0 = success. Non-zero = failure per FR-009. |
| `Output` | `string` | Captured stdout. May be empty. |
| `Error` | `string` | Captured stderr. May be empty. |

---

### RootMenuAction

Implements `IMenuAction`. Replaces `StubMenuAction("Root")` in `CategoryRegistry`. Owns the two-level Root navigation loop.

| Field | Type | Notes |
|---|---|---|
| `_registry` | `IRootItemRegistry` | Injected. |

`ExecuteAsync(IAnsiConsole console, NavigationContext navContext)` — two-level loop:

1. Show Root item selection (`SelectionPrompt<string>` with item display names + `"<- Back"`)
2. If `"<- Back"` → `navContext.GoBack()`, return
3. Show handler selection for chosen item (`SelectionPrompt<string>` with handler display names + `"<- Back"`)
4. If `"<- Back"` → continue (return to level 1)
5. Create `HandlerContext(workingDirectory)`, call `handler.ExecuteAsync(console, ctx)`, continue to level 1

---

## Dependency Graph

```
Program.cs
  └── AppRunner (IAnsiConsole, ICategoryRegistry, IMenuRenderer)
        └── ICategoryRegistry → CategoryRegistry
              └── Category["Root"] → RootMenuAction (IRootItemRegistry)
                    └── IRootItemRegistry → RootItemRegistry (ICommandRunner)
                          └── RootItem[]
                                └── IScaffoldHandler[] → DotnetNewHandler (ICommandRunner)
                                      └── ICommandRunner → ProcessCommandRunner
                                            └── CommandResult
```

`HandlerContext` is `new`-ed inline per handler invocation — not in DI.
`NavigationContext` (existing) remains unchanged.

---

## State Machine — Root Navigation

```
 ┌─────────────────────────────────────┐
 │   Top-Level Menu (AppRunner loop)   │
 └──────────────┬──────────────────────┘
                │ user selects "Root"
                ▼
 ┌─────────────────────────────────────┐
 │   Root Item Menu                    │◄─────────────────────────┐
 │   (.gitignore, .gitattributes, Back)│                          │
 └──────────────┬──────────────────────┘                          │
                │                                                  │
   ┌────────────┴──────────────┐                                   │
   │                           │                                   │
   ▼                           ▼                                   │
 "<- Back"               Item selected                             │
   │                           │                                   │
   ▼                           ▼                                   │
navCtx.GoBack()    ┌─────────────────────────────────┐            │
(→ top-level)      │   Handler Selection Menu         │            │
                   │  (dotnet new, ...future, Back)   │            │
                   └──────────────┬──────────────────┘            │
                                  │                                │
                     ┌────────────┴──────────┐                    │
                     │                       │                     │
                     ▼                       ▼                     │
                  "<- Back"           Handler selected             │
                     │                       │                     │
                     └──────────────┐        │                     │
                                    ▼        ▼                     │
                                 ┌──────────────────┐             │
                                 │ HandlerContext    │             │
                                 │ handler.Execute   │             │
                                 │ display output    │             │
                                 └────────┬─────────┘             │
                                          │ complete               │
                                          └───────────────────────┘
```
