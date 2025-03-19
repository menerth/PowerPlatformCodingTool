using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PPCT.Models;
using PPCT.Models.ConfigFiles;

namespace PPCT.Tasks.ModelbuilderTasks
{
    public class InitTask(AppInput appInput, ILogger<InitTask> log) : IPPCTTask
    {
        private readonly AppInput _appInput = appInput;
        private readonly ILogger<InitTask> _log = log;

        public Task<bool> Execute(CancellationToken ct)
        {
            _log.LogInformation("Initializing builder config file...");

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

            var builderConfig = new ModelbuilderConfig();

            var content = JsonConvert.SerializeObject(builderConfig, Formatting.Indented);

            File.WriteAllText(Path.Combine(configPath, "ppctbuilder.json"), content);

            return Task.FromResult(true);
        }
    }
}
