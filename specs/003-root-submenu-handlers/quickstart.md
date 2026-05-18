# Quickstart: Root Submenu with Command Handlers

**Feature**: 003-root-submenu-handlers

---

## Run the Tool

```powershell
scaffold
# Select: Root
# Select: .gitignore
# Select: dotnet new
# → runs "dotnet new gitignore" in current directory
```

---

## Run Tests

```powershell
# All tests
dotnet test

# Unit tests only
dotnet test --project tests\Scaffold.Cli.Tests\Scaffold.Cli.Tests.csproj

# E2E tests only
dotnet test --project tests\Scaffold.Cli.EndToEnd\Scaffold.Cli.EndToEnd.csproj
```

---

## Build & Pack

```powershell
dotnet build
dotnet pack src\Scaffold.Cli\Scaffold.Cli.csproj -o "$HOME\nuget-local" --configuration Release
dotnet tool uninstall --global Scaffold.Cli
dotnet tool install --global Scaffold.Cli --add-source "$HOME\nuget-local"
```

---

## Add a New Root Item (Using Existing Handler)

Edit `src/Scaffold.Cli/Root/RootItemRegistry.cs`. Add one entry above the closing `};`:

```csharp
new RootItem(".editorconfig", new List<IScaffoldHandler>
{
    new DotnetNewHandler("editorconfig", commandRunner)
}),
```

No other files need changing.

---

## Add a New Handler Type

1. Create `src/Scaffold.Cli/Handlers/MyNewHandler.cs`:

```csharp
public class MyNewHandler : IScaffoldHandler
{
    public string DisplayName => "my handler";

    public MyNewHandler(ICommandRunner commandRunner) { ... }

    public async Task ExecuteAsync(IAnsiConsole console, HandlerContext context)
    {
        // do work using console + context.WorkingDirectory
        context.GoBack();
    }
}
```

2. Register it in `RootItemRegistry` alongside existing handlers for a Root item:

```csharp
new RootItem(".gitignore", new List<IScaffoldHandler>
{
    new DotnetNewHandler("gitignore", commandRunner),
    new MyNewHandler(commandRunner),    // ← add here
}),
```

---

## Test a Handler with a Mock Command Runner

```csharp
var mockRunner = new StubCommandRunner(exitCode: 0, output: "File created.", error: "");
var handler = new DotnetNewHandler("gitignore", mockRunner);
var console = new TestConsole().Interactive();
var ctx = new HandlerContext(Directory.GetCurrentDirectory());

await handler.ExecuteAsync(console, ctx);

Assert.True(ctx.ShouldGoBack);
Assert.Contains("File created.", console.Output);
```
