using Scaffold.Cli.Actions;
using Scaffold.Cli.Categories;
using Scaffold.Cli.Root;

namespace Scaffold.Cli.Tests.Categories;

public class CategoryRegistryTests
{
    private class EmptyRootItemRegistry : IRootItemRegistry
    {
        public IReadOnlyList<RootItem> GetItems() => new List<RootItem>();
    }

    private static CategoryRegistry CreateRegistry() =>
        new(new ExitMenuAction(), new RootMenuAction(new EmptyRootItemRegistry()));

    [Fact]
    public void GetCategories_ReturnsFiveCategories()
    {
        Assert.Equal(5, CreateRegistry().GetCategories().Count);
    }

    [Fact]
    public void GetCategories_FirstCategoryIsAspire()
    {
        Assert.Equal("Aspire", CreateRegistry().GetCategories()[0].DisplayName);
    }

    [Fact]
    public void GetCategories_LastCategoryIsExit()
    {
        Assert.Equal("Exit", CreateRegistry().GetCategories()[^1].DisplayName);
    }

    [Fact]
    public void GetCategories_ExitAction_IsExitMenuAction()
    {
        Assert.IsType<ExitMenuAction>(CreateRegistry().GetCategories()[^1].Action);
    }

    [Fact]
    public void GetCategories_RootAction_IsRootMenuAction()
    {
        var rootCategory = CreateRegistry().GetCategories().First(c => c.DisplayName == "Root");
        Assert.IsType<RootMenuAction>(rootCategory.Action);
    }
}
