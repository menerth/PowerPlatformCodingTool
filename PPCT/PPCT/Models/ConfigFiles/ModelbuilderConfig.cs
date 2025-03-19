namespace PPCT.Models.ConfigFiles
{
    public class ModelbuilderConfig
    {
        public string OutDirectory { get; set; } = "./Models";
        public bool SuppressINotifyPattern { get; set; } = true;
        public bool SuppressGeneratedCodeAttribute { get; set; } = true;
        public string Language { get; set; } = "cs";
        public string Namespace { get; set; } = "PPCT.Models";
        public string ServiceContextName { get; set; } = "XrmContext";
        public bool GenerateSDKMessages { get; set; } = false;
        public bool GenerateGlobalOptionSets { get; set; } = true;
        public bool EmitFieldClasses { get; set; } = true;
        public bool EmitEntityETC { get; set; } = false;
        public bool EmitVirtualAttributes { get; set; } = false;
        public string EntityTypesFolder { get; set; } = "Entities";
        public string MessageTypesFolder { get; set; } = "Messages";
        public string OptionSetTypesFolder { get; set; } = "OptionSets";
        public List<string> SelectedEntitites { get; set; } = ["account", "contact"];
        public List<string> MessageNamesFilter { get; set; } = ["examp_*"];
        public bool ProcessForms { get; set; } = false;
        public BuilderCustomization Customization { get; set; } = new BuilderCustomization();
    }

    public class BuilderCustomization
    {
        public string EntityClassNamePattern { get; set; } = "{className}Model";
        public Dictionary<string,string> NamingOverrides { get; set; } = [];
    }
}
