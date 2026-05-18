using Scaffold.Cli.Categories;
using Spectre.Console;

namespace Scaffold.Cli.Menu;

public interface IMenuRenderer
{
    Task<Category> ShowMenuAsync(IReadOnlyList<Category> categories, string title);
}
