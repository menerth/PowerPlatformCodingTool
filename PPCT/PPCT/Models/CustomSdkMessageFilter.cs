using Microsoft.Xrm.Sdk;
using PPCT.Models.Dataverse;

namespace PPCT.Models
{
    public class CustomSdkMessageFilter
    {
        public string LogicalEntityName { get; set; }
        public EntityReference SdkMessageFilter { get; set; }
        public string MessageName { get; set; }
        public EntityReference SdkMessage { get; set; }
        public bool IsCustomApi { get; set; }
        public customapi_allowedcustomprocessingsteptype? AllowedCustomProcessingStepType { get; set; } = null;
    }
}
