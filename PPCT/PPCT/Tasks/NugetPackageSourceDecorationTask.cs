using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using NuGet.Packaging;
using PPCT.Components;
using PPCT.Models;
using PPCT.Models.Dataverse;
using PPCT.Services;

namespace PPCT.Tasks
{
    public class NugetPackageSourceDecorationTask : ICCPTTask
    {
        private readonly ServiceClient _serviceClient;
        private readonly ConsoleArgs _args;
        private readonly SolutionProcessor _scanner;
        private readonly IConfigurationFileLoader _configLoader;
        private readonly ILogger<NugetPackageSourceDecorationTask> _log;

        public NugetPackageSourceDecorationTask(ConsoleArgs consoleArgs, SolutionProcessor solutionScanner, IDataverseConnectionService dataverseConnectionService, IConfigurationFileLoader configurationFileLoader, ILogger<NugetPackageSourceDecorationTask> log)
        {
            _args = consoleArgs;
            _serviceClient = dataverseConnectionService.Client;
            _configLoader = configurationFileLoader;
            _scanner = solutionScanner;
            _log = log;
        }

        public async Task<bool> Execute(CancellationToken ct)
        {
            var config = _configLoader.LoadConfigurationFile<ConfigurationFile>();

            _log.LogTrace("Config file loaded:\n{content}", JsonConvert.SerializeObject(config, Formatting.Indented));

            var packageIds = GetPackagesIds(config).ToList();
            var solution = await DataverseMethods.GetSolutionInformation(_serviceClient, config.NugetPackage.DataverseSolutionName).ConfigureAwait(false);

            var searchStrings = packageIds.Select(x => $"{solution.publisher_solution.CustomizationPrefix}_{x}").ToList();

            var pluginData = await GetPluginTypesExpanded(searchStrings).ConfigureAwait(false);
            var customApiData = await GetCustomApiTypesExpanded(searchStrings).ConfigureAwait(false);

            var attributes = pluginData.Concat(customApiData).GroupBy(x => x.Item1).ToDictionary(x => x.Key, x => x.Select(y => y.Item2));

            _log.LogInformation("Found {count} plugin(s) and {count2} custom api(s)", pluginData.Count, customApiData.Count);

            await _scanner.ScanSolution(config.SolutionPath, attributes).ConfigureAwait(false);

            return true;
        }

        private IEnumerable<string> GetPackagesIds(ConfigurationFile config)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), config.NugetPackage.NugetPackagePath);
            _log.LogTrace("Searching for packages...");

            var packages = Directory.GetFiles(path, "*.nupkg");

            if (packages.Length == 0)
            {
                throw new Exception("No NuGet package(s) found, please build your solution first");
            }
            _log.LogInformation("Found {count} NuGet package(s)", packages.Length);

            foreach (var packagePath in packages)
            {
                using FileStream inputStream = new(packagePath, FileMode.Open);
                using PackageArchiveReader reader = new(inputStream);
                NuspecReader nuspec = reader.NuspecReader;

                var packageId = nuspec.GetId();
                yield return packageId;
            }
        }

        private async Task<List<Tuple<string, DataverseRegistrationAttribute>>> GetCustomApiTypesExpanded(List<string> searchStrings)
        {
            var query = new QueryExpression("customapi")
            {
                ColumnSet = new ColumnSet("uniquename"),
                LinkEntities =
                {
                    new LinkEntity("customapi", "plugintype", "plugintypeid", "plugintypeid", JoinOperator.Inner)
                    {
                        EntityAlias = "type",
                        Columns = new ColumnSet("typename"),
                        LinkEntities =
                        {
                            new LinkEntity("plugintype", "pluginassembly", "pluginassemblyid", "pluginassemblyid", JoinOperator.Inner)
                            {
                                LinkEntities =
                                {
                                    new LinkEntity(
                                        "pluginassembly",
                                        "pluginpackage",
                                        "packageid",
                                        "pluginpackageid",
                                        JoinOperator.Inner)
                                    {
                                        LinkCriteria =
                                        {
                                            Conditions =
                                            {
                                                new ConditionExpression("name", ConditionOperator.In, searchStrings)
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var result = await _serviceClient.RetrieveMultipleAsync(query).ConfigureAwait(false);

            if (result.Entities.Count == 0)
            {
                return [];
            }

            var attributes = result.Entities.Select(x => x.ToEntity<CustomAPI>()).Select(x =>
            {
                var attribute = new DataverseRegistrationAttribute(x.GetAttributeValue<string>(CustomAPI.Fields.UniqueName));
                attribute.RegistrationType = RegistrationTypeEnum.CustomApi;

                return new Tuple<string, DataverseRegistrationAttribute>(x.GetAttributeValue<AliasedValue>("type.typename").Value as string, attribute);
            }).ToList();

            return attributes;
        }


        private async Task<List<Tuple<string, DataverseRegistrationAttribute>>> GetPluginTypesExpanded(List<string> searchStrings)
        {
            var query = new QueryExpression(SdkMessageProcessingStep.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    SdkMessageProcessingStep.Fields.AsyncAutoDelete,
                    SdkMessageProcessingStep.Fields.Configuration,
                    SdkMessageProcessingStep.Fields.Description,
                    SdkMessageProcessingStep.Fields.FilteringAttributes,
                    SdkMessageProcessingStep.Fields.ImpersonatingUserId,
                    SdkMessageProcessingStep.Fields.Mode,
                    SdkMessageProcessingStep.Fields.Name,
                    SdkMessageProcessingStep.Fields.Rank,
                    SdkMessageProcessingStep.Fields.SdkMessageProcessingStepId,
                    SdkMessageProcessingStep.Fields.Stage,
                    SdkMessageProcessingStep.Fields.SupportedDeployment),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(SdkMessageProcessingStep.Fields.Category, ConditionOperator.NotEqual, "CustomAPI")
                    }
                },
                LinkEntities =
                {
                    new LinkEntity(SdkMessageProcessingStep.EntityLogicalName, PluginType.EntityLogicalName, SdkMessageProcessingStep.Fields.EventHandler, PluginType.Fields.Id, JoinOperator.Inner)
                    {
                        EntityAlias = "type",
                        Columns = new ColumnSet(PluginType.Fields.TypeName),
                        LinkEntities =
                        {
                            new LinkEntity(PluginType.EntityLogicalName, PluginAssembly.EntityLogicalName, PluginAssembly.Fields.Id, PluginType.Fields.PluginAssemblyId, JoinOperator.Inner)
                            {
                                EntityAlias = "pluginassembly",
                                Columns = new ColumnSet(PluginAssembly.Fields.IsolationMode),
                                LinkEntities =
                                {
                                    new LinkEntity(
                                        PluginAssembly.EntityLogicalName,
                                        pluginpackage.EntityLogicalName,
                                        PluginAssembly.Fields.PackageId,
                                        pluginpackage.Fields.Id,
                                        JoinOperator.Inner)
                                    {
                                        LinkCriteria =
                                        {
                                            Conditions =
                                            {
                                                new ConditionExpression(pluginpackage.Fields.name, ConditionOperator.In, searchStrings)
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new LinkEntity(SdkMessageProcessingStep.EntityLogicalName, SdkMessage.EntityLogicalName, SdkMessageProcessingStep.Fields.SdkMessageId, SdkMessage.Fields.Id, JoinOperator.Inner)
                    {
                        EntityAlias = "message",
                        Columns = new ColumnSet(SdkMessage.Fields.Name)
                    },
                    new LinkEntity(
                        SdkMessageProcessingStep.EntityLogicalName,
                        SdkMessageProcessingStepImage.EntityLogicalName,
                        SdkMessageProcessingStep.Fields.Id,
                        SdkMessageProcessingStepImage.Fields.SdkMessageProcessingStepId,
                        JoinOperator.LeftOuter)
                    {
                        EntityAlias = "image",
                        Columns = new ColumnSet(
                            SdkMessageProcessingStepImage.Fields.Id,
                            SdkMessageProcessingStepImage.Fields.Attributes1,
                            SdkMessageProcessingStepImage.Fields.Description,
                            SdkMessageProcessingStepImage.Fields.EntityAlias,
                            SdkMessageProcessingStepImage.Fields.ImageType,
                            SdkMessageProcessingStepImage.Fields.Name
                            )
                    },
                    new LinkEntity(
                        SdkMessageProcessingStep.EntityLogicalName,
                        SdkMessageFilter.EntityLogicalName,
                        SdkMessageProcessingStep.Fields.SdkMessageFilterId,
                        SdkMessageFilter.Fields.Id,
                        JoinOperator.LeftOuter)
                    {
                        EntityAlias = "filter",
                        Columns = new ColumnSet(SdkMessageFilter.Fields.PrimaryObjectTypeCode)
                    }
                }
            };

            var result = await _serviceClient.RetrieveMultipleAsync(query).ConfigureAwait(false);

            if (result.Entities.Count == 0)
            {
                return [];
            }


            var imageRelationship = new Relationship(SdkMessageProcessingStepImage.Fields.sdkmessageprocessingstepid_sdkmessageprocessingstepimage);

            var steps = result.Entities.GroupBy(x => x.Id);

            var attributes = new List<Tuple<string, DataverseRegistrationAttribute>>();

            foreach (var step in steps)
            {
                var stepRecord = step.First().ToEntity<SdkMessageProcessingStep>();

                stepRecord.plugintypeid_sdkmessageprocessingstep = new PluginType
                {
                    TypeName = step.First().GetAttributeValue<AliasedValue>("type.typename").Value.ToString()
                };

                stepRecord.sdkmessageid_sdkmessageprocessingstep = new SdkMessage
                {
                    Name = step.First().GetAttributeValue<AliasedValue>("message.name").Value.ToString()
                };

                var entityName = step.First().GetAttributeValue<AliasedValue>("filter.primaryobjecttypecode")?.Value as string ?? string.Empty;
                var isolationMode = step.First().GetAttributeValue<AliasedValue>("pluginassembly.isolationmode")?.Value as OptionSetValue;

                var images = new List<SdkMessageProcessingStepImage>();
                var imagesRaw = step.Where(x => x.GetAttributeValue<AliasedValue>("image.sdkmessageprocessingstepimageid") != null);
                if (imagesRaw.Any())
                {
                    var imagesRecords = imagesRaw.Select(x =>
                    {
                        var img = new SdkMessageProcessingStepImage()
                        {
                            Id = (Guid)x.GetAttributeValue<AliasedValue>("image.sdkmessageprocessingstepimageid").Value,
                            EntityAlias = x.GetAttributeValue<AliasedValue>("image.entityalias")?.Value as string,
                            Name = x.GetAttributeValue<AliasedValue>("image.name")?.Value as string,
                            Attributes1 = x.GetAttributeValue<AliasedValue>("image.attributes")?.Value as string,
                            Description = x.GetAttributeValue<AliasedValue>("image.description")?.Value as string,
                            ImageType = (sdkmessageprocessingstepimage_imagetype)(x.GetAttributeValue<AliasedValue>("image.imagetype").Value as OptionSetValue).Value
                        };

                        return img;
                    });

                    images.AddRange(imagesRecords);
                }

                var dataverseRegAttribute = new DataverseRegistrationAttribute(
                    stepRecord.sdkmessageid_sdkmessageprocessingstep.Name,
                    entityName,
                    (StageEnum)stepRecord.Stage,
                    stepRecord.Mode == sdkmessageprocessingstep_mode.Synchronous ? ExecutionModeEnum.Synchronous : ExecutionModeEnum.Asynchronous,
                    stepRecord.FilteringAttributes ?? string.Empty,
                    stepRecord.Name,
                    stepRecord.Rank ?? 1,
                    isolationMode.Value == (int)pluginassembly_isolationmode.Sandbox ? IsolationModeEnum.Sandbox : IsolationModeEnum.None,
                    stepRecord.Id.ToString())
                {
                    DeleteAsyncOperation = stepRecord.AsyncAutoDelete ?? false,
                };
                if (!string.IsNullOrEmpty(stepRecord.Description))
                {
                    dataverseRegAttribute.Description = stepRecord.Description;
                }

                if (!string.IsNullOrEmpty(stepRecord.Configuration))
                {
                    dataverseRegAttribute.UnSecureConfiguration = stepRecord.Configuration;
                }

                if (images.Count >= 1)
                {
                    var image = images.First();
                    dataverseRegAttribute.Image1Type = (ImageTypeEnum)Enum.ToObject(typeof(ImageTypeEnum), image.ImageType.Value);
                    dataverseRegAttribute.Image1Name = image.EntityAlias;
                    dataverseRegAttribute.Image1Attributes = image.Attributes1;
                }
                if (images.Count >= 2)
                {
                    var image = images.ElementAt(1);
                    dataverseRegAttribute.Image2Type = (ImageTypeEnum)Enum.ToObject(typeof(ImageTypeEnum), image.ImageType.Value);
                    dataverseRegAttribute.Image2Name = image.EntityAlias;
                    dataverseRegAttribute.Image2Attributes = image.Attributes1;
                }

                if (stepRecord.ImpersonatingUserId == null)
                {
                    dataverseRegAttribute.ExecuteAs = ImpersonationTypeEnum.CallingUser;
                }

                dataverseRegAttribute.RegistrationType = RegistrationTypeEnum.Plugin;

                attributes.Add(new Tuple<string, DataverseRegistrationAttribute>(stepRecord.plugintypeid_sdkmessageprocessingstep.TypeName, dataverseRegAttribute));
            }


            return attributes;
        }
    }
}
