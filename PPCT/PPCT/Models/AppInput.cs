using static PPCT.Models.Enums;

namespace PPCT.Models
{
    public class AppInput
    {
        public required Type TaskCategory { get; init; }
        public required int AppTask { get; init; }
        public AssembliesMode? AssemblyMode { get; init; } = null;
        public string? Namespace { get; init; } = null;
        public string? Path { get; init; } = null;

        public string TaskFlag => $"{TaskCategory.Name}-{AppTask}";
    }
}
