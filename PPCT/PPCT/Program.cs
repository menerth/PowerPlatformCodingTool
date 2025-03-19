using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PPCT;
using PPCT.Models;
using PPCT.Services;
using PPCT.Tasks;
using PPCT.Tasks.AssembliesTasks;
using PPCT.Tasks.ModelbuilderTasks;
using System.CommandLine;
using System.Reflection;
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

var setup = CommandLineSetup.Setup(args, out int exitCode);

if (exitCode == 0 && setup != null)
{
    var services = CreateServices(setup);

    var app = services.GetRequiredService<IApplication>();
    await app.Execute(cts.Token);
}

//var parser = new Parser(options =>
//{
//    options.CaseInsensitiveEnumValues = true;
//    options.HelpWriter = Console.Error;
//});

////var parseResult = parser.ParseArguments<ConsoleArgs>(args);

//var result = parser.ParseArguments(args, LoadVerbs());
//var command = result
//    .WithParsedAsync<AssembliesCommand>(async opts =>
//    {
//        Console.WriteLine(HelpText.AutoBuild(result, h => h, e => e));
//    });

//if (result.Tag == ParserResultType.Parsed)
//{

//}
//else
//{
//    Console.ForegroundColor = ConsoleColor.Cyan;

//    result.WithNotParsed(errors =>
//    {
//        errors.Output();
//    });

//    Environment.ExitCode = 1;
//}

//await parseResult.WithParsedAsync(async consoleArgs =>
//{
//    if (consoleArgs.Task == PPCTTask.None)
//    {
//        Console.WriteLine("Unknown task specified.");
//    }
//    else
//    {
//        var services = CreateServices(consoleArgs);

//        var app = services.GetRequiredService<IApplication>();
//        await app.Execute(cts.Token);
//    }
//});
//parseResult.WithNotParsed(errors =>
//{
//    Console.ForegroundColor = ConsoleColor.Cyan;
//    errors.Output();
//    Environment.ExitCode = 1;
//});

Console.ForegroundColor = ConsoleColor.Gray;

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

static ServiceProvider CreateServices(AppInput appInput)
{
    var serviceProvider = new ServiceCollection()
        .AddLogging(options =>
        {
            options.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });
        })
        .AddSingleton(appInput)
        .AddSingleton<IApplication, Application>()
        .AddSingleton<IDataverseConnectionService, DataverseConnectionService>()
        .AddSingleton<ITaskManager, TaskManager>()
        .AddSingleton<IConfigurationFileLoader, ConfigurationFileLoader>()
        .AddSingleton<NugetPackageScanner>()
        .AddSingleton<SolutionProcessor>()
        .AddKeyedTransient<IPPCTTask, AddTask>($"{typeof(PPCTAssembliesTask).Name}-{(int)PPCTAssembliesTask.Add}")
        .AddKeyedTransient<IPPCTTask, DeployTask>($"{typeof(PPCTAssembliesTask).Name}-{(int)PPCTAssembliesTask.Deploy}")
        .AddKeyedTransient<IPPCTTask, SourceDecorationTask>($"{typeof(PPCTAssembliesTask).Name}-{(int)PPCTAssembliesTask.Decorate}")
        .AddKeyedTransient<IPPCTTask, UpcomingGenericTask>($"{typeof(PPCTAssembliesTask).Name}-{(int)PPCTAssembliesTask.Init}")
        .AddKeyedTransient<IPPCTTask, InitTask>($"{typeof(PPCTModelBuilderTask).Name}-{(int)PPCTModelBuilderTask.Init}")
        .AddKeyedTransient<IPPCTTask, RunTask>($"{typeof(PPCTModelBuilderTask).Name}-{(int)PPCTModelBuilderTask.Run}")
        .AddKeyedTransient<IPPCTTask, UpcomingGenericTask>($"{typeof(PPCTMonitoringTask).Name}-{(int)PPCTMonitoringTask.Get}")
        .AddKeyedTransient<IPPCTTask, UpcomingGenericTask>($"{typeof(PPCTWebresourcesTask).Name}-{(int)PPCTWebresourcesTask.Init}")
        .BuildServiceProvider();

    return serviceProvider;
}
