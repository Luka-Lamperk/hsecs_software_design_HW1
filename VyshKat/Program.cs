using Microsoft.Extensions.DependencyInjection;
using VyshKat.ConsoleApp;
using VyshKat.Domain;

// Both normal and demo sessions use the same DI registrations. The demo receives
// its own repository and scripted console, so it cannot modify the user's park.
ServiceProvider CreateProvider(IConsoleIO io)
{
    var services = new ServiceCollection();
    services.AddSingleton<IConsoleIO>(io);
    services.AddSingleton<IAssetRepository, InMemoryAssetRepository>();
    services.AddSingleton<IServiceCenter, InteractiveServiceCenter>();
    services.AddSingleton<FleetService>();
    services.AddSingleton<DemoRunner>(_ => new DemoRunner((demoIo, action) =>
    {
        using var demoProvider = CreateProvider(demoIo);
        action(demoProvider.GetRequiredService<IApplicationUi>());
    }));
    services.AddSingleton<IApplicationUi, ApplicationUi>();
    return services.BuildServiceProvider();
}

using var provider = CreateProvider(new SystemConsoleIO());
try
{
    provider.GetRequiredService<IApplicationUi>().RunInteractive();
}
catch (EndOfStreamException) { Console.WriteLine("Ввод завершён."); }
