using Scaffold.Cli.Handlers;
using Scaffold.Cli.Root;
using Scaffold.Cli.Tests.Handlers;

namespace Scaffold.Cli.Tests.Root;

public class RootItemExtensibilityTests
{
    [Fact]
    public void AddingNewRootItem_OnlyRequiresRegistryChange()
    {
        var stub = new StubCommandRunner(0, "", "");
        var extraItem = new RootItem(".editorconfig", new List<IScaffoldHandler>
        {
            new DotnetNewHandler("editorconfig", stub)
        });

        var standardItems = new RootItemRegistry(stub).GetItems().ToList();
        standardItems.Add(extraItem);

        Assert.Equal(3, standardItems.Count);
        Assert.Contains(standardItems, i => i.DisplayName == ".editorconfig");
    }

    [Fact]
    public void ReusingDotnetNewHandler_RequiresNoNewClass()
    {
        var stub = new StubCommandRunner(0, "", "");
        var handler = new DotnetNewHandler("editorconfig", stub);
        Assert.Equal("dotnet new", handler.DisplayName);
    }
}
