namespace PPCT.Models
{
    public class ConfigurationFile
    {
        public required string SolutionPath { get; set; }
        public NugetFileConfig NugetPackage { get; set; }
    }

    public class  NugetFileConfig
    {
        public required string DataverseSolutionName { get; set; } = "YourSolution";
        public required string NugetPackagePath { get; set; }
    }
}
