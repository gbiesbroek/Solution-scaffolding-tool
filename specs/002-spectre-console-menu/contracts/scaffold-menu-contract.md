# CLI Contract: scaffold Interactive Menu

**Feature**: 002-spectre-console-menu | **Date**: 2026-05-18

---

## Invocation Contract

```
scaffold
```

No arguments, flags, or environment variables are read or required. The tool always starts in
interactive menu mode.

---

## Startup Behaviour

| Property | Value |
|----------|-------|
| Startup output | Renders the top-level `SelectionPrompt` — title + 5 category lines visible |
| Time to first render | < 1 second on a standard developer workstation |
| Interactive requirement | Must run in an interactive terminal with ANSI support |

---

## Top-Level Menu Contract

```
? What would you like to scaffold?
> Aspire
  Frontend
  Core
  Root
  Exit
```

- The title prompt text is: `"What would you like to scaffold?"` (subject to implementation; testable via `TestConsole.Output`)
- Arrow keys navigate the selection
- Enter confirms the highlighted choice
- No "← Back" entry is present at the top level
- The menu wraps around (pressing Up on first item moves to last; pressing Down on last moves to first)

---

## Exit Behaviour

| Trigger | Outcome |
|---------|---------|
| User selects **Exit** from the top-level menu | Process terminates, exit code **0** |
| Ctrl+C at any level | Process terminates, standard interrupt exit code |

---

## Sub-Context Contract (Stub — current iteration)

When the user selects Aspire, Frontend, Core, or Root:

1. A placeholder message is displayed (exact text TBD by implementation).
2. A "← Back" entry appears as the only menu choice.
3. Selecting "← Back" returns to the top-level menu without restarting the process.

The sub-context is not a separate process or shell — it is rendered in the same terminal session.

---

## IMenuAction Interface Contract

Any future implementation of `IMenuAction` MUST:

1. Accept `IAnsiConsole console` and `NavigationContext context` as parameters.
2. Either call `context.Exit()` (to signal program termination) or `context.GoBack()` (to return to parent menu) before returning.
3. NOT call `Environment.Exit()` directly — navigation is always delegated to `NavigationContext`.
4. Be registered in the DI container or constructed by `CategoryRegistry`.

---

## ICategoryRegistry Contract

Any implementation of `ICategoryRegistry` MUST:

1. Return a non-empty `IReadOnlyList<Category>` from `GetCategories()`.
2. Always include an entry whose `IMenuAction` calls `context.Exit()` as the last item.
3. Return the list in the intended display order.
4. Return the same list on every call (stable, deterministic).

---

## IMenuRenderer Contract

Any implementation of `IMenuRenderer` MUST:

1. Render the provided `IReadOnlyList<Category>` using the injected `IAnsiConsole`.
2. Return exactly one `Category` from the provided list (no nulls, no out-of-list items).
3. Not mutate `NavigationContext` — that is the responsibility of `IMenuAction`.
4. Add a `"← Back"` synthetic entry when `includeBack: true` is passed (for sub-context menus).

---

## Testability Contract

All production menu and action classes MUST accept `IAnsiConsole` via constructor injection.
Tests MUST use `Spectre.Console.Testing.TestConsole` as the `IAnsiConsole` implementation.
No class may call static `AnsiConsole.*` methods directly.
