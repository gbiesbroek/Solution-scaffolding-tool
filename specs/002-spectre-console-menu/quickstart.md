# Quickstart: scaffold Interactive Category Menu

**Feature**: 002-spectre-console-menu

---

## Prerequisites

- .NET 10 SDK (`dotnet --version` → `10.x.x`)
- `scaffold` global tool installed from feature 001 (or re-install per `specs/001-cli-nuget-tool/quickstart.md`)
- Repository cloned, on branch `002-spectre-console-menu`

---

## Running the Tool

```bash
scaffold
```

Expected output — an interactive menu:
```
? What would you like to scaffold?
> Aspire
  Frontend
  Core
  Root
  Exit
```

Use ↑ / ↓ to navigate, Enter to select.  
Selecting **Exit** terminates with exit code 0.  
Selecting any other category shows a placeholder and a **← Back** option.

---

## Running Tests

```powershell
# Unit tests (fast — uses TestConsole, no process launch)
dotnet test --project tests\Scaffold.Cli.Tests\Scaffold.Cli.Tests.csproj

# End-to-end tests (launches real binary)
dotnet test --project tests\Scaffold.Cli.EndToEnd\Scaffold.Cli.EndToEnd.csproj

# All tests
dotnet test
```

---

## Building & Packing

```powershell
# Build
dotnet build

# Pack to local NuGet feed
dotnet pack src\Scaffold.Cli\Scaffold.Cli.csproj -o "$HOME\nuget-local" --configuration Release

# Reinstall global tool
dotnet tool uninstall --global Scaffold.Cli
dotnet tool install --global Scaffold.Cli --add-source "$HOME\nuget-local"
```

---

## Adding a New Category

1. Open `src\Scaffold.Cli\Categories\CategoryRegistry.cs`
2. Add a new `Category` entry to the list — before the **Exit** entry:

```csharp
new("Infrastructure", new StubMenuAction("Infrastructure")),
```

3. Run `dotnet build` to verify — no other files need changing.
4. Run `dotnet test` to confirm existing tests still pass.

---

## Adding a Real Action (Future)

1. Create a new class implementing `IMenuAction`:

```csharp
public class InfrastructureMenuAction : IMenuAction
{
    private readonly IAnsiConsole _console;

    public InfrastructureMenuAction(IAnsiConsole console)
        => _console = console;

    public Task ExecuteAsync(IAnsiConsole console, NavigationContext context)
    {
        // show sub-menu, etc.
        context.GoBack();
        return Task.CompletedTask;
    }
}
```

2. Register it in `CategoryRegistry` (or resolve from DI if it has dependencies).
3. Write unit tests using `TestConsole`:

```csharp
var console = new TestConsole().Interactive();
console.Input.PushKey(ConsoleKey.Enter); // confirm first item
var context = new NavigationContext();
await new InfrastructureMenuAction().ExecuteAsync(console, context);
Assert.True(context.ShouldGoBack);
```

---

## Project Structure

```
src/Scaffold.Cli/
├── Program.cs                        # DI setup + AppRunner invocation
├── AppRunner.cs                      # navigation loop
├── Infrastructure/
│   └── ServiceCollectionExtensions.cs  # AddScaffoldServices()
├── Menu/
│   ├── IMenuRenderer.cs
│   ├── MenuRenderer.cs               # wraps SelectionPrompt<Category>
│   └── NavigationContext.cs
├── Categories/
│   ├── Category.cs
│   ├── ICategoryRegistry.cs
│   └── CategoryRegistry.cs           # THE single place to add categories
└── Actions/
    ├── IMenuAction.cs
    ├── ExitMenuAction.cs
    └── StubMenuAction.cs

tests/Scaffold.Cli.Tests/             # Unit tests (TestConsole)
tests/Scaffold.Cli.EndToEnd/          # E2E tests (process launch)
```
