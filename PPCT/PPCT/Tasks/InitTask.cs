using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PPCT.Models;

namespace PPCT.Tasks
{
    public class InitTask(ConsoleArgs args, ILogger<InitTask> log) : ICCPTTask
    {
        private readonly ILogger<InitTask> _log = log;
        private readonly ConsoleArgs _args = args;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<bool> Execute(CancellationToken ct = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            _log.LogInformation("Initializing config file...");

            var ppctConfigPath = Directory.GetCurrentDirectory() + "/ppct.json";

            var initConfig = new ConfigurationFile()
            {
                SolutionPath = "YourSolution.sln",
                NugetPackage =
                        new()
                        {
                            DataverseSolutionName = "YourPowerPlatformSolutionName",
                            NugetPackagePath = "project\\bin\\outputPackages"
                        }
            };

            var content = JsonConvert.SerializeObject(initConfig, Formatting.Indented);

            var ppctDecoratePath = Directory.GetCurrentDirectory() + "/decorate.bat";
            var ppctDeployPath = Directory.GetCurrentDirectory() + "/deploy.bat";

            try
            {
                File.WriteAllText(ppctConfigPath, content);

                _log.LogInformation("Config file generated!");

                File.WriteAllText(ppctDecoratePath, "ppct -t extract -v");
                File.WriteAllText(ppctDeployPath, "ppct -t deploy -v");

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
