using Scaffold.Cli.Actions;
using Scaffold.Cli.Categories;

namespace Scaffold.Cli.Tests.Categories;

public class CategoryRegistryTests
{
    [Fact]
    public void GetCategories_ReturnsFiveCategories()
    {
        var registry = new CategoryRegistry(new ExitMenuAction());
        Assert.Equal(5, registry.GetCategories().Count);
    }

    [Fact]
    public void GetCategories_FirstCategoryIsAspire()
    {
        var registry = new CategoryRegistry(new ExitMenuAction());
        Assert.Equal("Aspire", registry.GetCategories()[0].DisplayName);
    }

    [Fact]
    public void GetCategories_LastCategoryIsExit()
    {
        var registry = new CategoryRegistry(new ExitMenuAction());
        Assert.Equal("Exit", registry.GetCategories()[^1].DisplayName);
    }

    [Fact]
    public void GetCategories_ExitAction_IsExitMenuAction()
    {
        var registry = new CategoryRegistry(new ExitMenuAction());
        Assert.IsType<ExitMenuAction>(registry.GetCategories()[^1].Action);
    }
}
