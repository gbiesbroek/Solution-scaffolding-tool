using Scaffold.Cli.Actions;
using Scaffold.Cli.Categories;

namespace Scaffold.Cli.Tests.Categories;

public class CategoryExtensibilityTests
{
    [Fact]
    public void AddingNewCategory_AppearsInMenu()
    {
        var exitAction = new ExitMenuAction();
        var extraCategory = new Category("Infrastructure", new StubMenuAction("Infrastructure"));

        var categories = new List<Category>
        {
            new("Aspire", new StubMenuAction("Aspire")),
            new("Frontend", new StubMenuAction("Frontend")),
            new("Core", new StubMenuAction("Core")),
            new("Root", new StubMenuAction("Root")),
            extraCategory,
            new("Exit", exitAction),
        };

        Assert.Equal(6, categories.Count);
        Assert.Contains(categories, c => c.DisplayName == "Infrastructure");
    }

    [Fact]
    public void Category_Description_IsOptionalAndRoundTrips()
    {
        var cat = new Category("Test", new ExitMenuAction()) { Description = "A description" };
        Assert.Equal("A description", cat.Description);
    }
}
