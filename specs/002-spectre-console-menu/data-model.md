# Data Model: Spectre.Console Interactive Category Menu

**Feature**: 002-spectre-console-menu | **Date**: 2026-05-18

---

## Entities

### Category

Represents a single entry in a menu. Carries a display name and the follow-up action to invoke
when selected. Designed to accommodate additional properties in future iterations without changing
the menu infrastructure.

| Field | Type | Notes |
|-------|------|-------|
| `DisplayName` | `string` | Shown in the menu list. Required, non-empty. |
| `Action` | `IMenuAction` | Invoked when this category is selected. Required. |

> Future iterations may add: `Description`, `Icon`, `Order`, `Tags`, etc. — the entity is the
> extension point; the menu renderer is not.

---

### IMenuAction

Abstraction for the behaviour triggered when a category is selected.

| Member | Signature | Notes |
|--------|-----------|-------|
| `ExecuteAsync` | `Task ExecuteAsync(IAnsiConsole console, NavigationContext context)` | Renders the follow-up experience and mutates `context` to signal navigation intent. |

**Known implementations:**

| Class | Behaviour |
|-------|-----------|
| `ExitMenuAction` | Calls `context.Exit()` — signals the main loop to terminate. |
| `StubMenuAction` | Displays a placeholder message and calls `context.GoBack()` immediately. |

---

### NavigationContext

Per-frame mutable state passed to every `IMenuAction.ExecuteAsync` call. Carries signals for
the navigation loop.

| Field / Method | Type | Notes |
|----------------|------|-------|
| `ShouldExit` | `bool` (read-only) | Set to `true` by calling `Exit()`. Tells the main loop to stop. |
| `ShouldGoBack` | `bool` (read-only) | Set to `true` by calling `GoBack()`. Tells the loop to redisplay the parent menu. |
| `Exit()` | `void` | Sets `ShouldExit = true`. |
| `GoBack()` | `void` | Sets `ShouldGoBack = true`. |

A new `NavigationContext` instance is created for each `ExecuteAsync` invocation. State is not
shared across invocations.

---

### ICategoryRegistry / CategoryRegistry

The single authoritative source for the ordered list of categories.

| Member | Signature | Notes |
|--------|-----------|-------|
| `GetCategories()` | `IReadOnlyList<Category>` | Returns all top-level categories in display order. |

`CategoryRegistry` builds the list once (constructor or lazy init). The initial list, in order:
1. Aspire → `StubMenuAction`
2. Frontend → `StubMenuAction`
3. Core → `StubMenuAction`
4. Root → `StubMenuAction`
5. Exit → `ExitMenuAction`

Adding a new category requires editing `CategoryRegistry` only.

---

### IMenuRenderer / MenuRenderer

Responsible for rendering the `SelectionPrompt<Category>` and returning the user's selection.
Delegates all terminal I/O to an injected `IAnsiConsole`.

| Member | Signature | Notes |
|--------|-----------|-------|
| `ShowMenuAsync` | `Task<Category> ShowMenuAsync(IReadOnlyList<Category> categories, string title)` | Renders the prompt and returns the selected `Category`. |

`MenuRenderer` does not know about navigation — it only presents choices and returns a result.

---

## State Machine — Navigation Loop

```
 ┌──────────────────────────────────────┐
 │          App Starts                  │
 └──────────────┬───────────────────────┘
                │
                ▼
 ┌──────────────────────────────────────┐
 │   Show Top-Level Menu                │◄──────────────┐
 │   (no "← Back" entry)               │               │
 └──────────────┬───────────────────────┘               │
                │ user selects category                  │
                ▼                                        │
 ┌──────────────────────────────────────┐               │
 │   Create NavigationContext           │               │
 │   Invoke category.Action.ExecuteAsync│               │
 └──────────┬───────────────────────────┘               │
            │                                           │
   ┌────────┴────────┐                                  │
   │                 │                                  │
   ▼                 ▼                                  │
ShouldExit=true   ShouldGoBack=true                     │
   │                 │                                  │
   ▼                 └──────────────────────────────────┘
 ┌──────────────────────────────────────┐
 │   App Exits (exit code 0)            │
 └──────────────────────────────────────┘
```

**Sub-context state machine** (for future non-stub actions):

```
 ┌──────────────────────────────────────┐
 │   Show Sub-Context Menu              │◄──────────────┐
 │   (last item is "← Back")           │               │
 └──────────────┬───────────────────────┘               │
                │                                        │
   ┌────────────┴───────────────┐                       │
   │                            │                       │
   ▼                            ▼                       │
 User selects             User selects "← Back"         │
 non-Back item            → context.GoBack()            │
   │                            │                       │
   ▼                            └───────────────────────┘
 [Future: nested sub-action]   (returns to parent menu)
```

---

## Dependency Graph

```
Program.cs
  └── AppRunner (IAnsiConsole, ICategoryRegistry, IMenuRenderer)
        ├── IMenuRenderer → MenuRenderer (IAnsiConsole)
        ├── ICategoryRegistry → CategoryRegistry
        │     └── Category[]
        │           └── IMenuAction (ExitMenuAction | StubMenuAction)
        └── NavigationContext (created per-invocation)
```

All nodes except `NavigationContext` are registered in the DI container.
`NavigationContext` is `new`-ed inline — it is a per-frame value object.
