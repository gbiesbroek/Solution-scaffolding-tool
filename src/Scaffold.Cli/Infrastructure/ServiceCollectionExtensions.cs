using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Scaffold.Cli.Actions;
using Scaffold.Cli.Categories;
using Scaffold.Cli.Handlers;
using Scaffold.Cli.Menu;
using Scaffold.Cli.Root;

namespace Scaffold.Cli.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScaffoldServices(this IServiceCollection services)
    {
        services.AddSingleton<IAnsiConsole>(AnsiConsole.Console);
        services.AddSingleton<ICommandRunner, ProcessCommandRunner>();
        services.AddSingleton<ExitMenuAction>();
        services.AddSingleton<IRootItemRegistry>(sp =>
            new RootItemRegistry(sp.GetRequiredService<ICommandRunner>()));
        services.AddSingleton<RootMenuAction>(sp =>
            new RootMenuAction(sp.GetRequiredService<IRootItemRegistry>()));
        services.AddSingleton<ICategoryRegistry>(sp =>
            new CategoryRegistry(
                sp.GetRequiredService<ExitMenuAction>(),
                sp.GetRequiredService<RootMenuAction>()));
        services.AddSingleton<IMenuRenderer>(sp =>
            new MenuRenderer(sp.GetRequiredService<IAnsiConsole>()));
        services.AddTransient<AppRunner>();
        return services;
    }
}
