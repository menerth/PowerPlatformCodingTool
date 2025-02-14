using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PPCT.Models;
using PPCT.Services;
using PPCT.Tasks;
using static PPCT.Models.Enums;

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (sender, eventArgs) =>
{
    Console.WriteLine("Canceling...");
    eventArgs.Cancel = true;
    cts.Cancel();
};

Console.ForegroundColor = ConsoleColor.DarkYellow;
Console.WriteLine("===============================================================");
Console.WriteLine($"        Power Platform Coding Tool");
Console.WriteLine("===============================================================");
Console.WriteLine("""
                               ____     ____      ____   _____   
                             U|  _"\ uU|  _"\ uU /"___| |_ " _|  
                             \| |_) |/\| |_) |/\| | u     | |    
                              |  __/   |  __/   | |/__   /| |\   
                              |_|      |_|       \____| u |_|U   
                              ||>>_    ||>>_    _// \\  _// \\_  
                             (__)__)  (__)__)  (__)(__)(__) (__) 
                         """);
Console.WriteLine("===============================================================");
Console.ForegroundColor = ConsoleColor.Gray;

var parser = new Parser(options =>
{
    options.CaseInsensitiveEnumValues = true;
    options.HelpWriter = Console.Error;
});

var parseResult = parser.ParseArguments<ConsoleArgs>(args);
await parseResult.WithParsedAsync(async consoleArgs =>
{
    if (consoleArgs.Task == PPCTTask.None)
    {
        Console.WriteLine("Unknown task specified.");
    }
    else
    {
        var services = CreateServices(consoleArgs);

        var app = services.GetRequiredService<IApplication>();
        await app.Execute(cts.Token);
    }
});
parseResult.WithNotParsed(errors =>
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    errors.Output();
    Environment.ExitCode = 1;
});

Console.ForegroundColor = ConsoleColor.Gray;

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

static ServiceProvider CreateServices(ConsoleArgs args)
{
    var serviceProvider = new ServiceCollection()
        .AddSingleton(args)
        .AddLogging(options =>
        {
            options.SetMinimumLevel(args.Verbose ? LogLevel.Trace : LogLevel.Information);
            options.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });
        })
        .AddSingleton<IApplication, Application>()
        .AddSingleton<IDataverseConnectionService, DataverseConnectionService>()
        .AddSingleton<ITaskManager, TaskManager>()
        .AddSingleton<IConfigurationFileLoader, ConfigurationFileLoader>()
        .AddSingleton<NugetPackageScanner>()
        .AddSingleton<SolutionProcessor>()
        .AddKeyedTransient<ICCPTTask, InitTask>(PPCTTask.Init)
        .AddKeyedTransient<ICCPTTask, NugetPackageDeployTask>(PPCTTask.Deploy)
        .AddKeyedTransient<ICCPTTask, NugetPackageSourceDecorationTask>(PPCTTask.Extract)
        .BuildServiceProvider();

    return serviceProvider;
}
