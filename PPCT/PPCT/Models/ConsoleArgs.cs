using CommandLine;
using static PPCT.Models.Enums;

namespace PPCT.Models
{
    public class ConsoleArgs
    {
        private const string HelpText = $@"
init = Create ppct.json file
deploy = Deploy NuGet Package to Power Platform
extract = Download data from dependent assemblies and add code attributes to existing plugin classes";

        [Option('h', "help", Required = false, HelpText = "")]
        public bool Help { get; init; } = false;

        [Option('t', "task", Required = true, HelpText = HelpText)]
        public PPCTTask Task { get; init; } = PPCTTask.None;

        [Option('p', "path", Required = false, HelpText = "Optional path to ppct.json file")]
        public string Path { get; init; } = string.Empty;

        [Option('v', "verbose", Required = false, HelpText = "More detailed logging")]
        public bool Verbose { get; init; } = false;
    }
}
