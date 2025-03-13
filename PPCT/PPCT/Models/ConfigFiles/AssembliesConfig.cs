using static PPCT.Models.Enums;

namespace PPCT.Models.ConfigFiles
{
    public class AssembliesConfig
    {
        public required string SolutionPath { get; set; }
        public required AssembliesArtifactConfig Artifact { get; set; }
        public required string Mode { get; set; }

    }

    public class AssembliesArtifactConfig
    {
        public required string DataverseSolutionName { get; set; } = "YourSolution";
        public required string BuildArtifactPath { get; set; }
    }
}
