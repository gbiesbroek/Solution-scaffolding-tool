using Microsoft.Extensions.DependencyInjection;
using Scaffold.Cli;
using Scaffold.Cli.Infrastructure;

await using var services = new ServiceCollection()
    .AddScaffoldServices()
    .BuildServiceProvider();

var runner = services.GetRequiredService<AppRunner>();
await runner.RunAsync();
