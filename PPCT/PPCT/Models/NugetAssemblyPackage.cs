using PPCT.Components;
using static PPCT.Models.Enums;

namespace PPCT.Models
{
    public class NugetAssemblyPackage
    {
        public int EntityTypeCode { get; set; }

        public string Base64Content { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public ProcessingAction ProcessingAction { get; set; } = ProcessingAction.Unknown;
        public bool PackageUploaded { get; set; } = false;

        public List<NugetAssembly> Assemblies { get; set; } = [];
    }

    public class NugetAssembly
    {
        public string AssemblyName { get; set; }

        public List<RegisterableClass> RegisterableClasses { get; set; } = [];
    }

    public class RegisterableClass
    {
        public Type ClassType { get; set; }

        public ProcessingAction ProcessingAction { get; set; } = ProcessingAction.Unknown;

        public List<DataverseRegistrationAttribute> Attributes { get; set; } = [];
    }
}
