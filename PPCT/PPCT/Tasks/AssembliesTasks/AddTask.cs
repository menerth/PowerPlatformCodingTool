using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PPCT.Models;
using PPCT.Models.ConfigFiles;
using static PPCT.Models.Enums;

namespace PPCT.Tasks.AssembliesTasks
{
    public class AddTask(AppInput appInput, ILogger<AddTask> log) : IPPCTTask
    {
        private readonly ILogger<AddTask> _log = log;
        private readonly AppInput _appInput = appInput;

        public Task<bool> Execute(CancellationToken ct = default)
        {
            _log.LogInformation("Initializing config file...");

            string configPath;
            if (string.IsNullOrWhiteSpace(_appInput.Path))
            {
                configPath = Directory.GetCurrentDirectory();
            }
            else
            {
                configPath = Path.IsPathFullyQualified(_appInput.Path)
                    ? _appInput.Path
                    : Path.Combine(Directory.GetCurrentDirectory(), _appInput.Path);
            }

            var addConfig = new AssembliesConfig()
            {
                Mode = (_appInput.AssemblyMode.HasValue ? (AssembliesMode)_appInput.AssemblyMode : AssembliesMode.Nuget).ToString().ToLowerInvariant(),
                SolutionPath = "YourSolution.sln",
                Artifact = new()
                {
                    BuildArtifactPath = "project\\bin\\output",
                    DataverseSolutionName = "YourPowerPlatformSolutionName",
                }
            };

            var content = JsonConvert.SerializeObject(addConfig, Formatting.Indented);

            var ppctDecoratePath = Path.Combine(configPath, "decorate.bat");
            var ppctDeployPath = Path.Combine(configPath, "deploy.bat");

            try
            {
                File.WriteAllText(Path.Combine(configPath, "ppct.json"), content);

                _log.LogInformation("Config file generated!");

                File.WriteAllText(ppctDecoratePath, "ppct assemblies decorate");
                File.WriteAllText(ppctDeployPath, "ppct assemblies deploy");

                return Task.FromResult(true);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
