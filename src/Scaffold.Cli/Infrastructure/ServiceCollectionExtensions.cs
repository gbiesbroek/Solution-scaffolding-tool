using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Scaffold.Cli.Actions;
using Scaffold.Cli.Categories;
using Scaffold.Cli.Menu;

namespace Scaffold.Cli.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScaffoldServices(this IServiceCollection services)
    {
        services.AddSingleton<IAnsiConsole>(AnsiConsole.Console);
        services.AddSingleton<ExitMenuAction>();
        services.AddSingleton<ICategoryRegistry>(sp =>
            new CategoryRegistry(sp.GetRequiredService<ExitMenuAction>()));
        services.AddSingleton<IMenuRenderer>(sp =>
            new MenuRenderer(sp.GetRequiredService<IAnsiConsole>()));
        services.AddTransient<AppRunner>();
        return services;
    }
}
