using PPCT.Models;
using System.CommandLine;
using static PPCT.Models.Enums;

namespace PPCT
{
    public static class CommandLineSetup
    {
        public static AppInput? Setup(string[] args, out int exitCode)
        {
            AppInput? appInput = null;

            var pathOption = new Option<string>(["-p", "--path"], "Path")
            {
                IsRequired = false,
                Arity = ArgumentArity.ExactlyOne,
                AllowMultipleArgumentsPerToken = false,
            };

            var namespaceOption = new Option<string>(["-ns", "--namespace"], "Namespace")
            {
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne,
                AllowMultipleArgumentsPerToken = false,
            };

            var assembliesModeOption = new Option<AssembliesMode>(["-m", "--mode"], () => AssembliesMode.Nuget, "Mode")
            {
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne,
                AllowMultipleArgumentsPerToken = false,
            };

            var rootCommand = new RootCommand("PPCT");

            var assembliesMainCommand = new Command("assemblies", "Work with assemblies");

            var assembliesAddCommand = new Command("add", "Add PPCT tooling to existing project");
            assembliesAddCommand.AddOption(assembliesModeOption);
            assembliesAddCommand.AddOption(pathOption);
            assembliesAddCommand.SetHandler((context) =>
            {
                appInput = new AppInput
                {
                    TaskCategory = typeof(PPCTAssembliesTask),
                    AppTask = (int)PPCTAssembliesTask.Add,
                    AssemblyMode = context.ParseResult.GetValueForOption(assembliesModeOption),
                    Path = context.ParseResult.GetValueForOption(pathOption),
                };
            });

            var assembliesDecorateCommand = new Command("decorate", "Decorate existing plugin classes with PPCT attributes");
            assembliesDecorateCommand.AddOption(pathOption);
            assembliesDecorateCommand.SetHandler((context) =>
            {
                appInput = new AppInput
                {
                    TaskCategory = typeof(PPCTAssembliesTask),
                    AppTask = (int)PPCTAssembliesTask.Decorate,
                    Path = context.ParseResult.GetValueForOption(pathOption),
                };
            });

            var assembliesDeployCommand = new Command("deploy", "Deploy plugins to Power Platform");
            assembliesDeployCommand.AddOption(pathOption);
            assembliesDeployCommand.SetHandler((context) =>
            {
                appInput = new AppInput
                {
                    TaskCategory = typeof(PPCTAssembliesTask),
                    AppTask = (int)PPCTAssembliesTask.Deploy,
                    Path = context.ParseResult.GetValueForOption(pathOption),
                };
            });

            var assembliesInitCommand = new Command("init", "Create template project (upcoming)");
            assembliesInitCommand.AddOption(namespaceOption);
            assembliesInitCommand.AddOption(pathOption);

            assembliesInitCommand.SetHandler((context) =>
            {
                appInput = new AppInput
                {
                    TaskCategory = typeof(PPCTAssembliesTask),
                    AppTask = (int)PPCTAssembliesTask.Init,
                    Namespace = context.ParseResult.GetValueForOption(namespaceOption),
                    Path = context.ParseResult.GetValueForOption(pathOption),
                };

            });

            assembliesMainCommand.Add(assembliesAddCommand);
            assembliesMainCommand.Add(assembliesDecorateCommand);
            assembliesMainCommand.Add(assembliesDeployCommand);
            assembliesMainCommand.Add(assembliesInitCommand);

            rootCommand.Add(assembliesMainCommand);

            var modelbuilderMainCommand = new Command("modelbuilder", "Work with modelbuilder (upcoming)");

            var modelbuilderAddCommand = new Command("init", "Create PPCT tooling config file to be used for running modelbuilder");
            modelbuilderAddCommand.AddOption(pathOption);
            modelbuilderAddCommand.SetHandler((context) =>
            {
                appInput = new AppInput
                {
                    TaskCategory = typeof(PPCTModelBuilderTask),
                    AppTask = (int)PPCTModelBuilderTask.Init,
                    Path = context.ParseResult.GetValueForOption(pathOption),
                };
            });

            var modelbuilderRunCommand = new Command("run", "Run modelbuilder (upcoming)");
            modelbuilderRunCommand.AddOption(pathOption);
            modelbuilderRunCommand.SetHandler((context) =>
            {
                appInput = new AppInput
                {
                    TaskCategory = typeof(PPCTModelBuilderTask),
                    AppTask = (int)PPCTModelBuilderTask.Run,
                    Path = context.ParseResult.GetValueForOption(pathOption),
                };
            });

            modelbuilderMainCommand.Add(modelbuilderAddCommand);
            modelbuilderMainCommand.Add(modelbuilderRunCommand);
            rootCommand.Add(modelbuilderMainCommand);

            var monitoringMainCommand = new Command("monitoring", "Work with monitoring (upcoming)");

            var monitoringGetCommand = new Command("get", "Get template files to deploy into Azure to be used for monitoring (upcoming)");
            monitoringGetCommand.AddOption(pathOption);
            monitoringGetCommand.SetHandler((context) =>
            {
                appInput = new AppInput
                {
                    TaskCategory = typeof(PPCTMonitoringTask),
                    AppTask = (int)PPCTMonitoringTask.Get,
                    Path = context.ParseResult.GetValueForOption(pathOption),
                };
            });

            monitoringMainCommand.Add(monitoringGetCommand);
            rootCommand.Add(monitoringMainCommand);

            var webresourcesMainCommand = new Command("webresources", "Work with webresources (upcoming)");

            var webresourcesAddCommand = new Command("init", "Initialize template project for webresources (upcoming)");
            webresourcesAddCommand.AddOption(pathOption);
            webresourcesAddCommand.SetHandler((context) =>
            {
                appInput = new AppInput
                {
                    TaskCategory = typeof(PPCTWebresourcesTask),
                    AppTask = (int)PPCTWebresourcesTask.Init,
                    Path = context.ParseResult.GetValueForOption(pathOption),
                };
            });

            webresourcesMainCommand.Add(webresourcesAddCommand);
            rootCommand.Add(webresourcesMainCommand);

            exitCode = rootCommand.Invoke(args);

            return appInput;
        }
    }
}
