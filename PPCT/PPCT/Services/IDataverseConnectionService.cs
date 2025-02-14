using Microsoft.PowerPlatform.Dataverse.Client;

namespace PPCT.Services
{
    public interface IDataverseConnectionService
    {
        ServiceClient Client { get; }
    }
}