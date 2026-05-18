namespace Scaffold.Cli.Categories;

public interface ICategoryRegistry
{
    IReadOnlyList<Category> GetCategories();
}
