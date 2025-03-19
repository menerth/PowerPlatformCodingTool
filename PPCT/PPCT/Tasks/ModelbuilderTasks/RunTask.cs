using Microsoft.Extensions.Logging;
using PPCT.Models.ConfigFiles;
using PPCT.Models;
using PPCT.Services;
using Newtonsoft.Json;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;

namespace PPCT.Tasks.ModelbuilderTasks
{
    public class RunTask(AppInput appInput, IDataverseConnectionService dataverseConnectionService, IConfigurationFileLoader configurationFileLoader, ILogger<RunTask> log) : IPPCTTask
    {
        private readonly AppInput _appInput = appInput;
        private readonly ServiceClient _dvClient = dataverseConnectionService.Client;
        private readonly IConfigurationFileLoader _configLoader = configurationFileLoader;
        private readonly ILogger<RunTask> _log = log;

        public async Task<bool> Execute(CancellationToken ct)
        {
            var config = _configLoader.LoadConfigurationFile<ModelbuilderConfig>(_appInput.Path);
            _log.LogTrace("Config file loaded:\n{content}", JsonConvert.SerializeObject(config, Formatting.Indented));

            var entitiesMetadata = await GeEntitiesMetadata(config.SelectedEntitites);

            var globalOptionSets = config.GenerateGlobalOptionSets ? await GetGlobalOptionSets() : [];

            return true;
        }

        private async Task<List<EntityMetadata>> GeEntitiesMetadata(IEnumerable<string> entityNames)
        {
            if (!entityNames.Any())
            {
                var allRequest = new RetrieveAllEntitiesRequest()
                {
                    EntityFilters = EntityFilters.Entity | EntityFilters.Attributes | EntityFilters.Relationships,
                    RetrieveAsIfPublished = false,
                };

                var response = (await _dvClient.ExecuteAsync(allRequest)) as RetrieveAllEntitiesResponse;
                return [.. response?.EntityMetadata];
            }
            else
            {
                var selectedResults = new List<EntityMetadata>();
                var chunks = entityNames.Chunk(20);

                foreach (var chunk in chunks)
                {
                    var filter = new MetadataFilterExpression(LogicalOperator.Or);
                    filter.Conditions.AddRange(chunk.Select(e => new MetadataConditionExpression("logicalname", MetadataConditionOperator.Equals, e)));

                    var subRequest = new RetrieveMetadataChangesRequest()
                    {
                        Query = new EntityQueryExpression()
                        {
                            Criteria = filter,
                            Properties = new MetadataPropertiesExpression()
                            {
                                AllProperties = true,
                            }
                        }
                    };

                    selectedResults.AddRange((await _dvClient.ExecuteAsync(subRequest) as RetrieveMetadataChangesResponse)?.EntityMetadata ?? []);
                }

                return selectedResults;
            }

        }

        private async Task<OptionSetMetadataBase[]> GetGlobalOptionSets()
        {
            var req = new RetrieveAllOptionSetsRequest()
            {
                RetrieveAsIfPublished = false
            };

            return (await _dvClient.ExecuteAsync(req) as RetrieveAllOptionSetsResponse)?.OptionSetMetadata ?? [];
        }
    }
}
