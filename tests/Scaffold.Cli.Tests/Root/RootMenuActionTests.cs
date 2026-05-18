using Scaffold.Cli.Handlers;
using Scaffold.Cli.Menu;
using Scaffold.Cli.Root;
using Spectre.Console;
using Spectre.Console.Testing;

namespace Scaffold.Cli.Tests.Root;

public class RootMenuActionTests
{
    private class StubRootItemRegistry : IRootItemRegistry
    {
        private readonly IReadOnlyList<RootItem> _items;
        public StubRootItemRegistry(IReadOnlyList<RootItem> items) => _items = items;
        public IReadOnlyList<RootItem> GetItems() => _items;
    }

    private class RecordingHandler : IScaffoldHandler
    {
        public string DisplayName => "test handler";
        public string Preview => "Test preview";
        public bool WasCalled { get; private set; }
        public Task ExecuteAsync(IAnsiConsole console, HandlerContext context)
        {
            WasCalled = true;
            context.GoBack();
            return Task.CompletedTask;
        }
    }

    private static IRootItemRegistry OneItemRegistry(IScaffoldHandler handler) =>
        new StubRootItemRegistry(new List<RootItem>
        {
            new(".gitignore", new List<IScaffoldHandler> { handler })
        });

    [Fact]
    public async Task ExecuteAsync_BackSelected_CallsNavContextGoBack()
    {
        var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);

        var registry = OneItemRegistry(new RecordingHandler());
        var action = new RootMenuAction(registry);
        var navCtx = new NavigationContext();

        await action.ExecuteAsync(console, navCtx);

        Assert.True(navCtx.ShouldGoBack);
    }

    [Fact]
    public async Task ExecuteAsync_HandlerSelected_ExecutesHandler()
    {
        var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);

        var handler = new RecordingHandler();
        var registry = OneItemRegistry(handler);
        var action = new RootMenuAction(registry);
        var navCtx = new NavigationContext();

        await action.ExecuteAsync(console, navCtx);

        Assert.True(handler.WasCalled);
        Assert.True(navCtx.ShouldGoBack);
    }

    [Fact]
    public async Task ExecuteAsync_HandlerBackSelected_ReturnsToRootMenu()
    {
        var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);

        var registry = OneItemRegistry(new RecordingHandler());
        var action = new RootMenuAction(registry);
        var navCtx = new NavigationContext();

        await action.ExecuteAsync(console, navCtx);

        Assert.True(navCtx.ShouldGoBack);
    }

    [Fact]
    public async Task ExecuteAsync_HandlerSelected_BackOnConfirm_HandlerNotCalled()
    {
        var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);

        var handler = new RecordingHandler();
        var registry = OneItemRegistry(handler);
        var action = new RootMenuAction(registry);
        var navCtx = new NavigationContext();

        await action.ExecuteAsync(console, navCtx);

        Assert.False(handler.WasCalled);
        Assert.True(navCtx.ShouldGoBack);
    }

    [Fact]
    public async Task ExecuteAsync_HandlerSelected_ConfirmSelected_HandlerCalled()
    {
        var console = new TestConsole().Interactive();
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);

        var handler = new RecordingHandler();
        var registry = OneItemRegistry(handler);
        var action = new RootMenuAction(registry);
        var navCtx = new NavigationContext();

        await action.ExecuteAsync(console, navCtx);

        Assert.True(handler.WasCalled);
        Assert.True(navCtx.ShouldGoBack);
    }
}
