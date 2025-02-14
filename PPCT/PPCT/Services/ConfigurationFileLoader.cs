using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PPCT.Services
{
    public class ConfigurationFileLoader(ILogger<ConfigurationFileLoader> log) : IConfigurationFileLoader
    {
        private readonly ILogger<ConfigurationFileLoader> _log = log;

        public T LoadConfigurationFile<T>()
        {
            var ppctConfigPath = Directory.GetCurrentDirectory() + "\\ppct.json";

            _log.LogTrace("Loading configuration file from {path}", ppctConfigPath);

            string content;
            try
            {
                content = File.ReadAllText(ppctConfigPath);
            }
            catch (FileNotFoundException fex)
            {
                _log.LogError("File not found: {message}", fex.Message);
                throw new Exception("Configuration file not found!");
            }
            catch (Exception ex)
            {
                _log.LogError("Error reading configuration file: {message}", ex.Message);
                throw new Exception("Error reading configuration file!");
            }

            try
            {
                var config = JsonConvert.DeserializeObject<T>(content);
                return config;
            }
            catch (Exception ex)
            {
                _log.LogError("Error deserializing configuration file: {message}", ex.Message);
                throw new Exception("Error deserializing configuration file!");
            }
        }
    }
}
