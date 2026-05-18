using Scaffold.Cli.Handlers;
using Scaffold.Cli.Root;
using Scaffold.Cli.Tests.Handlers;

namespace Scaffold.Cli.Tests.Root;

public class RootItemRegistryTests
{
    private static RootItemRegistry CreateRegistry()
    {
        var stub = new StubCommandRunner(0, "", "");
        return new RootItemRegistry(stub, new StubFileSystem(), new StubHttpFileDownloader("content"));
    }

    [Fact]
    public void GetItems_ReturnsTwoItems()
    {
        Assert.Equal(2, CreateRegistry().GetItems().Count);
    }

    [Fact]
    public void GetItems_FirstItemIsGitignore()
    {
        Assert.Equal(".gitignore", CreateRegistry().GetItems()[0].DisplayName);
    }

    [Fact]
    public void GetItems_SecondItemIsGitattributes()
    {
        Assert.Equal(".gitattributes", CreateRegistry().GetItems()[1].DisplayName);
    }

    [Fact]
    public void GetItems_EachItemHasAtLeastOneHandler()
    {
        foreach (var item in CreateRegistry().GetItems())
            Assert.NotEmpty(item.Handlers);
    }

    [Fact]
    public void GetItems_GitignoreFirstHandler_IsDotnetNewHandler()
    {
        Assert.IsType<DotnetNewHandler>(CreateRegistry().GetItems()[0].Handlers[0]);
    }

    [Fact]
    public void GetItems_GitignoreHasTwoHandlers()
    {
        Assert.Equal(2, CreateRegistry().GetItems()[0].Handlers.Count);
    }

    [Fact]
    public void GetItems_GitignoreSecondHandler_IsWebToFileHandler()
    {
        Assert.IsType<WebToFileHandler>(CreateRegistry().GetItems()[0].Handlers[1]);
    }
}
