using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using PPCT.Components;
using PPCT.Models;
using PPCT.Models.Dataverse;
using PPCT.Services;
using static PPCT.Models.Enums;

namespace PPCT.Tasks
{
    public class NugetPackageDeployTask : ICCPTTask
    {
        private readonly ILogger<NugetPackageDeployTask> _log;
        private readonly ServiceClient _serviceClient;
        private readonly OrganizationServiceContext _ctx;
        private readonly ConsoleArgs _args;
        private readonly NugetPackageScanner _processor;
        private readonly IConfigurationFileLoader _configLoader;

        public NugetPackageDeployTask(ConsoleArgs consoleArgs, IDataverseConnectionService dataverseConnectionService, NugetPackageScanner processor, IConfigurationFileLoader configurationFileLoader, ILogger<NugetPackageDeployTask> log)
        {
            _args = consoleArgs;
            _serviceClient = dataverseConnectionService.Client;
            _ctx = new OrganizationServiceContext(_serviceClient) { MergeOption = MergeOption.NoTracking };
            _processor = processor;
            _configLoader = configurationFileLoader;
            _log = log;
        }

        public async Task<bool> Execute(CancellationToken ct = default)
        {

            var config = _configLoader.LoadConfigurationFile<ConfigurationFile>();

            _log.LogTrace("Config file loaded:\n{content}", JsonConvert.SerializeObject(config, Formatting.Indented));
            var nugetPackageTypeCode = GetPluginPackageTypeCode();
            var packages = _processor.ScanPackages(config.NugetPackage, nugetPackageTypeCode);
            var solution = await DataverseMethods.GetSolutionInformation(_serviceClient, config.NugetPackage.DataverseSolutionName).ConfigureAwait(false);

            foreach (var package in packages)
            {
                var customConfigurations = await ValidatePackageDeploymentConfig(package).ConfigureAwait(false);

                var nugetPackageRecord = await RegisterNuGetPackage(package, solution, ct);

                _log.LogInformation("Nuget package {name} registered with version {version}", package.Name, nugetPackageRecord.Version);

                await RegisterCustomAPIsLogicImplementations(nugetPackageRecord, package, ct).ConfigureAwait(false);

                await RegisterPluginLogicImplementations(nugetPackageRecord, package, customConfigurations, solution, ct).ConfigureAwait(false);
            }

            return true;
        }

        private async Task<List<CustomSdkMessageFilter>> ValidatePackageDeploymentConfig(NugetAssemblyPackage package)
        {
            var regAttributes = package.Assemblies.SelectMany(a => a.RegisterableClasses.SelectMany(c => c.Attributes)).Where(x => x.RegistrationType == RegistrationTypeEnum.Plugin);

            var customConfigs = await GetCustomConfigurations(regAttributes).ConfigureAwait(false);

            var invalidAttributes = new List<DataverseRegistrationAttribute>();
            foreach (var attr in regAttributes)
            {
                CustomSdkMessageFilter config;

                if (string.IsNullOrEmpty(attr.EntityLogicalName) || attr.EntityLogicalName == "none")
                {
                    config = customConfigs.FirstOrDefault(c => string.IsNullOrEmpty(c.LogicalEntityName) && c.MessageName == attr.Message);
                }
                else
                {
                    config = customConfigs.FirstOrDefault(c => c.LogicalEntityName == attr.EntityLogicalName && c.MessageName == attr.Message);
                }

                if (config == null)
                {
                    invalidAttributes.Add(attr);
                    continue;
                }

                if (config.IsCustomApi)
                {
                    if (config.AllowedCustomProcessingStepType == customapi_allowedcustomprocessingsteptype.None)
                    {
                        invalidAttributes.Add(attr);
                    }
                    else if (config.AllowedCustomProcessingStepType == customapi_allowedcustomprocessingsteptype.AsyncOnly && attr.ExecutionMode == ExecutionModeEnum.Synchronous && attr.Stage != null)
                    {
                        invalidAttributes.Add(attr);
                    }
                }
            }

            if (invalidAttributes.Count != 0)
            {
                throw new Exception("Some configrations to deploy plugins are not executable, please review your attributes");
            }

            return customConfigs;
        }
        private async Task<List<CustomSdkMessageFilter>> GetCustomConfigurations(IEnumerable<DataverseRegistrationAttribute> regAttributes)
        {
            var customConfigs = new List<CustomSdkMessageFilter>();
            customConfigs.AddRange(await GetBoundAllowedConfigurations(regAttributes).ConfigureAwait(false));
            customConfigs.AddRange(await GetUnboundAllowedConfigurations(regAttributes).ConfigureAwait(false));

            return customConfigs;
        }

        private async Task<List<CustomSdkMessageFilter>> GetBoundAllowedConfigurations(IEnumerable<DataverseRegistrationAttribute> regAttributes)
        {
            var sdkMessageAlias = "sdkm";
            var customApiAlias = "customApi";

            var entityBoundValidationFilters = regAttributes.Where(x => !string.IsNullOrEmpty(x.EntityLogicalName)).GroupBy(x => x.EntityLogicalName)
                .Select(x => new { EntityName = x.Key, Messages = x.Select(m => m.Message).Distinct() })
                .Select(x =>
                {
                    var filter = new FilterExpression(LogicalOperator.And);
                    filter.AddCondition(new ConditionExpression(SdkMessageFilter.Fields.PrimaryObjectTypeCode, ConditionOperator.Equal, x.EntityName));
                    var subFilter = new FilterExpression(LogicalOperator.Or);
                    filter.AddFilter(subFilter);
                    foreach (var message in x.Messages)
                    {
                        subFilter.AddCondition(new ConditionExpression(sdkMessageAlias, SdkMessage.Fields.Name, ConditionOperator.Equal, message));
                    }
                    return filter;
                });

            if (!entityBoundValidationFilters.Any())
            {
                return [];
            }

            var boundChunks = entityBoundValidationFilters.Chunk(20);

            var boundRecords = new List<Entity>();

            foreach (var chunk in boundChunks)
            {
                var boundQuery = new QueryExpression(SdkMessageFilter.EntityLogicalName)
                {
                    ColumnSet = new ColumnSet(SdkMessageFilter.Fields.Id, SdkMessageFilter.Fields.PrimaryObjectTypeCode),
                };
                var message = boundQuery.AddLink(SdkMessage.EntityLogicalName, SdkMessageFilter.Fields.SdkMessageId, SdkMessage.Fields.Id);
                message.EntityAlias = sdkMessageAlias;
                message.Columns = new ColumnSet(SdkMessage.Fields.Id, SdkMessage.Fields.Name);
                var mainFilter = new FilterExpression(LogicalOperator.Or);
                boundQuery.Criteria.AddFilter(mainFilter);
                mainFilter.Filters.AddRange(chunk);
                var customApi = message.AddLink(CustomAPI.EntityLogicalName, SdkMessage.Fields.Id, CustomAPI.Fields.SdkMessageId, JoinOperator.LeftOuter);
                customApi.EntityAlias = customApiAlias;
                customApi.Columns = new ColumnSet(CustomAPI.Fields.UniqueName, CustomAPI.Fields.BindingType, CustomAPI.Fields.AllowedCustomProcessingStepType);

                var result = await _serviceClient.RetrieveMultipleAsync(boundQuery).ConfigureAwait(false);

                boundRecords.AddRange(result.Entities);
            }

            if (boundRecords.Count == 0)
            {
                return [];
            }

            var customConfigs = boundRecords.Select(x =>
            {
                var config = new CustomSdkMessageFilter()
                {
                    SdkMessageFilter = x.ToEntityReference(),
                    LogicalEntityName = x.GetAttributeValue<string>(SdkMessageFilter.Fields.PrimaryObjectTypeCode),
                    MessageName = x.GetAttributeValue<AliasedValue>($"{sdkMessageAlias}.{SdkMessage.Fields.Name}").Value as string,
                    SdkMessage = new EntityReference(SdkMessage.EntityLogicalName, (Guid)x.GetAttributeValue<AliasedValue>($"{sdkMessageAlias}.{SdkMessageFilter.Fields.SdkMessageId}").Value),
                    IsCustomApi = x.GetAttributeValue<AliasedValue>($"{customApiAlias}.{CustomAPI.Fields.UniqueName}") != null
                };
                if (config.IsCustomApi)
                {
                    config.AllowedCustomProcessingStepType = (customapi_allowedcustomprocessingsteptype)((x.GetAttributeValue<AliasedValue>($"{customApiAlias}.{CustomAPI.Fields.AllowedCustomProcessingStepType}").Value as OptionSetValue).Value);
                }

                return config;
            }).ToList();

            return customConfigs;
        }

        private async Task<List<CustomSdkMessageFilter>> GetUnboundAllowedConfigurations(IEnumerable<DataverseRegistrationAttribute> regAttributes)
        {
            var customApiAlias = "customApi";

            var unboundValidationInput = regAttributes.Where(x => string.IsNullOrEmpty(x.EntityLogicalName)).Select(x => x.Message).Distinct().ToList();

            if (!unboundValidationInput.Any())
            {
                return [];
            }

            var unboundQuery = new QueryExpression(SdkMessage.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(SdkMessage.Fields.Id, SdkMessage.Fields.Name),
            };
            unboundQuery.Criteria.AddCondition(new ConditionExpression(SdkMessage.Fields.Name, ConditionOperator.In, unboundValidationInput));
            var custApi = unboundQuery.AddLink(CustomAPI.EntityLogicalName, SdkMessage.Fields.Id, CustomAPI.Fields.SdkMessageId, JoinOperator.LeftOuter);
            custApi.EntityAlias = customApiAlias;
            custApi.Columns = new ColumnSet(CustomAPI.Fields.UniqueName, CustomAPI.Fields.BindingType, CustomAPI.Fields.AllowedCustomProcessingStepType);

            var unboundResult = (await _serviceClient.RetrieveMultipleAsync(unboundQuery).ConfigureAwait(false)).Entities;

            var customConfigs = unboundResult.Select(x =>
            {
                var config = new CustomSdkMessageFilter()
                {
                    SdkMessageFilter = null,
                    LogicalEntityName = string.Empty,
                    MessageName = x.GetAttributeValue<string>(SdkMessage.Fields.Name),
                    SdkMessage = x.ToEntityReference(),
                    IsCustomApi = x.GetAttributeValue<AliasedValue>($"{customApiAlias}.{CustomAPI.Fields.UniqueName}") != null,
                };
                if (config.IsCustomApi)
                {
                    config.AllowedCustomProcessingStepType = (customapi_allowedcustomprocessingsteptype)((x.GetAttributeValue<AliasedValue>($"{customApiAlias}.{CustomAPI.Fields.AllowedCustomProcessingStepType}").Value as OptionSetValue).Value);
                }
                return config;

            }).ToList();

            return customConfigs;
        }

        private async Task RegisterPluginLogicImplementations(pluginpackage nugetPackageRecord, NugetAssemblyPackage package, List<CustomSdkMessageFilter> customConfigurations, Solution solution, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var existingSteps = GetPackagePluginSteps(nugetPackageRecord, ct);
            var existingTypes = GetExistingPluginTypes(nugetPackageRecord, ct);
            var existingImages = await GetExistingImages(existingSteps, ct).ConfigureAwait(false);

            foreach (var assembly in package.Assemblies)
            {
                foreach (var registerableClass in assembly.RegisterableClasses)
                {
                    foreach (var attribute in registerableClass.Attributes)
                    {
                        if (attribute.RegistrationType == RegistrationTypeEnum.Plugin)
                        {
                            _log.LogInformation("Registering plugin step for entity {entity} {name}", attribute.EntityLogicalName, attribute.Name);
                            ct.ThrowIfCancellationRequested();
                            SdkMessageProcessingStep runningStep = new();

                            var step = existingSteps.FirstOrDefault(x => x.Id == attribute?.StepId);
                            step ??= existingSteps.FirstOrDefault(x => x.Name == attribute.Name && x.sdkmessageid_sdkmessageprocessingstep.Name == attribute.Message && x.plugintypeid_sdkmessageprocessingstep.pluginassembly_plugintype.Name == assembly.AssemblyName);
                            step ??= null;

                            if (step != null)
                            {
                                runningStep.Id = step.Id;
                            }

                            if (string.IsNullOrEmpty(attribute.EntityLogicalName) || attribute.EntityLogicalName == "none")
                            {
                                runningStep.SdkMessageId = customConfigurations.FirstOrDefault(x => x.MessageName == attribute.Message).SdkMessage;
                            }
                            else
                            {
                                var config = customConfigurations.FirstOrDefault(x => x.MessageName == attribute.Message && x.LogicalEntityName == attribute.EntityLogicalName);
                                runningStep.SdkMessageId = config?.SdkMessage;
                                runningStep.SdkMessageFilterId = config?.SdkMessageFilter;
                            }

                            runningStep.EventHandler = existingTypes.FirstOrDefault(x => x.TypeName == registerableClass.ClassType.FullName && x.pluginassembly_plugintype.Name == assembly.AssemblyName).ToEntityReference();

                            runningStep.Name = attribute.Name;
                            runningStep.Configuration = attribute.UnSecureConfiguration;
                            runningStep.Description = attribute.Description;
                            runningStep.Mode = attribute.ExecutionMode == ExecutionModeEnum.Synchronous ? sdkmessageprocessingstep_mode.Synchronous : sdkmessageprocessingstep_mode.Asynchronous;
                            runningStep.AsyncAutoDelete = attribute.ExecutionMode == ExecutionModeEnum.Asynchronous && attribute.DeleteAsyncOperation;
                            runningStep.Rank = attribute.ExecutionOrder;

                            runningStep.Stage = attribute.Stage switch
                            {
                                StageEnum.PreValidation => (sdkmessageprocessingstep_stage?)sdkmessageprocessingstep_stage.Prevalidation,
                                StageEnum.PreOperation => (sdkmessageprocessingstep_stage?)sdkmessageprocessingstep_stage.Preoperation,
                                StageEnum.PostOperation => (sdkmessageprocessingstep_stage?)sdkmessageprocessingstep_stage.Postoperation,
                                _ => throw new Exception("Unsupported stage"),
                            };

                            runningStep.SupportedDeployment = (attribute.Server, attribute.Offline) switch
                            {
                                (true, true) => sdkmessageprocessingstep_supporteddeployment.Both,
                                (false, true) => sdkmessageprocessingstep_supporteddeployment.MicrosoftDynamics365ClientforOutlookOnly,
                                (_, _) => sdkmessageprocessingstep_supporteddeployment.ServerOnly
                            };
                            runningStep.FilteringAttributes = string.IsNullOrEmpty(attribute.FilteringAttributes) ? string.Empty : attribute.FilteringAttributes.Replace(" ", "");

                            if (runningStep.Id == Guid.Empty)
                            {
                                runningStep.Id = await _serviceClient.CreateAsync(step, ct).ConfigureAwait(false);
                            }
                            else
                            {
                                await _serviceClient.UpdateAsync(runningStep, ct).ConfigureAwait(false);
                            }

                            await RegisterStepImages(step, existingImages, attribute, ct).ConfigureAwait(false);

                            _log.LogInformation("Plugin step {name} registered", attribute.Name);

                            await AddPluginStepToSolution(solution.UniqueName, step.Id, ct).ConfigureAwait(false);

                        }
                    }
                }
            }
        }

        private async Task RegisterStepImages(SdkMessageProcessingStep step, List<SdkMessageProcessingStepImage> existingImages, DataverseRegistrationAttribute attribute, CancellationToken ct = default)
        {
            var currentStepImages = existingImages.Where(x => x.SdkMessageProcessingStepId.Id == step.Id).ToList();

            var incomingImages = new List<SdkMessageProcessingStepImage>();

            if (string.IsNullOrEmpty(attribute.Image1Name))
            {
                incomingImages.Add(PrepareImage(step, attribute, attribute.Image1Name, attribute.Image1Type, attribute.Image1Attributes));
            }
            if (string.IsNullOrEmpty(attribute.Image2Name))
            {
                var secondImage = PrepareImage(step, attribute, attribute.Image2Name, attribute.Image2Type, attribute.Image2Attributes);
            }

            foreach (var incomingImage in incomingImages)
            {
                var currentImage = currentStepImages.FirstOrDefault(x => x.EntityAlias == incomingImage.EntityAlias && x.ImageType == incomingImage.ImageType);
                if (currentImage != null)
                {
                    incomingImage.Id = currentImage.Id;
                }
                currentStepImages.Remove(currentImage);
            }

            foreach (var image in currentStepImages)
            {
                await _serviceClient.DeleteAsync(image.LogicalName, image.Id, ct).ConfigureAwait(false);
            }

        }

        private SdkMessageProcessingStepImage PrepareImage(SdkMessageProcessingStep step, DataverseRegistrationAttribute attribute, string imageName, ImageTypeEnum imageType, string filteringAttributes)
        {
            var image = new SdkMessageProcessingStepImage
            {
                Id = Guid.Empty,
                SdkMessageProcessingStepId = step.ToEntityReference(),
                Name = imageName,
                ImageType = (sdkmessageprocessingstepimage_imagetype)imageType,
                Attributes1 = filteringAttributes,
                EntityAlias = imageName,
                MessagePropertyName = attribute.Message switch
                {
                    "Create" => "Id",
                    "SetState" or "SetStateDynamicEntity" => "EntityMoniker",
                    "Send" or "DeliverIncoming" or "DeliverPromote" => "EmailId",
                    "CreateMultiple" or "UpdateMultiple" or "UpsertMultiple" or "DeleteMultiple" => "Targets",
                    _ => "Target",
                }
            };

            return image;
        }


        private async Task<List<SdkMessageProcessingStepImage>> GetExistingImages(List<SdkMessageProcessingStep> pluginSteps, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _log.LogInformation("Obtaining plugin step images...");

            if (pluginSteps.Count == 0)
            {
                return [];
            }

            var query = new QueryExpression(SdkMessageProcessingStepImage.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    SdkMessageProcessingStepImage.Fields.Id,
                    SdkMessageProcessingStepImage.Fields.Name,
                    SdkMessageProcessingStepImage.Fields.Attributes1,
                    SdkMessageProcessingStepImage.Fields.EntityAlias,
                    SdkMessageProcessingStepImage.Fields.ImageType,
                    SdkMessageProcessingStepImage.Fields.SdkMessageProcessingStepId
                    ),
            };
            query.Criteria.AddCondition(new ConditionExpression(SdkMessageProcessingStepImage.Fields.SdkMessageProcessingStepId, ConditionOperator.In, pluginSteps.Select(x => x.Id).ToList()));

            var images = await _serviceClient.RetrieveMultipleAsync(query).ConfigureAwait(false);

            var pluginImages = images.Entities
                            .Select(x => x.ToEntity<SdkMessageProcessingStepImage>()).ToList();

            _log.LogInformation("Plugin step images obtained.");
            return pluginImages;
        }

        private async Task RegisterCustomAPIsLogicImplementations(pluginpackage nugetPackageRecord, NugetAssemblyPackage package, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var packageCustomAPIs = package.Assemblies.SelectMany(a => a.RegisterableClasses.Where(c => c.Attributes.Any(attr => attr.RegistrationType == RegistrationTypeEnum.CustomApi)))
                .SelectMany(c => c.Attributes
                    .Select(attr => new { TypeName = c.ClassType.FullName, attr.Message }))
                .ToDictionary(x => x.Message, x => x.TypeName);

            if (packageCustomAPIs.Count == 0)
            {
                return;
            }

            var customAPIs = await GetCustomAPIs(packageCustomAPIs, ct).ConfigureAwait(false);
            var pluginTypes = await GetPluginTypes(packageCustomAPIs, ct).ConfigureAwait(false);

            foreach (var api in customAPIs)
            {
                _log.LogInformation("Updating Custom API {name}", api.UniqueName);
                var pluginType = pluginTypes.FirstOrDefault(x => x.TypeName == packageCustomAPIs.FirstOrDefault(y => y.Key == api.UniqueName).Value);

                if (api.PluginTypeId == null || api.PluginTypeId.Id != pluginType?.Id)
                {
                    var updatedApi = new CustomAPI()
                    {
                        Id = api.Id,
                        PluginTypeId = pluginType.ToEntityReference()
                    };

                    await _serviceClient.UpdateAsync(updatedApi, ct).ConfigureAwait(false);
                }
            }
        }

        private async Task<List<PluginType>> GetPluginTypes(Dictionary<string, string> packageCustomAPIs, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var query = new QueryExpression(PluginType.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(PluginType.Fields.Id, PluginType.Fields.TypeName),
                Criteria =
                    {
                        Conditions =
                        {
                            new ConditionExpression(PluginType.Fields.TypeName, ConditionOperator.In, packageCustomAPIs.Values.Distinct().ToList())
                        }
                    }
            };

            return (await _serviceClient.RetrieveMultipleAsync(query, ct).ConfigureAwait(false)).Entities.Select(x => x.ToEntity<PluginType>()).ToList();

        }

        private async Task<List<CustomAPI>> GetCustomAPIs(Dictionary<string, string> packageCustomAPIs, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var query = new QueryExpression(CustomAPI.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(CustomAPI.Fields.Id, CustomAPI.Fields.PluginTypeId, CustomAPI.Fields.UniqueName),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(CustomAPI.Fields.UniqueName, ConditionOperator.In, packageCustomAPIs.Keys.ToList())
                    }
                }
            };

            return (await _serviceClient.RetrieveMultipleAsync(query, ct).ConfigureAwait(false)).Entities.Select(x => x.ToEntity<CustomAPI>()).ToList();
        }

        private List<SdkMessageProcessingStep> GetPackagePluginSteps(pluginpackage nugetPackage, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _log.LogInformation("Obtaining plugin steps...");
            var steps = from pType in _ctx.CreateQuery<PluginType>()
                        join pStep in _ctx.CreateQuery<SdkMessageProcessingStep>()
                            on pType.Id equals pStep.EventHandler.Id
                        join pAss in _ctx.CreateQuery<PluginAssembly>()
                            on pType.PluginAssemblyId.Id equals pAss.PluginAssemblyId
                        join mes in _ctx.CreateQuery<SdkMessage>()
                            on pStep.SdkMessageId.Id equals mes.SdkMessageId
                        where pAss.PackageId.Id == nugetPackage.Id
                        select new SdkMessageProcessingStep()
                        {
                            Id = pStep.Id,
                            SdkMessageProcessingStepId = pStep.SdkMessageProcessingStepId,
                            Name = pStep.Name,
                            Mode = pStep.Mode,
                            FilteringAttributes = pStep.FilteringAttributes,
                            Rank = pStep.Rank,
                            Stage = pStep.Stage,
                            SdkMessageFilterId = pStep.SdkMessageFilterId,
                            Configuration = pStep.Configuration,
                            Description = pStep.Description,
                            ImpersonatingUserId = pStep.ImpersonatingUserId,
                            sdkmessageid_sdkmessageprocessingstep = new SdkMessage()
                            {
                                Id = mes.Id,
                                Name = mes.Name
                            },
                            plugintypeid_sdkmessageprocessingstep = new PluginType()
                            {
                                TypeName = pType.TypeName,
                                Id = pType.Id,
                                pluginassembly_plugintype = new PluginAssembly()
                                {
                                    IsolationMode = pAss.IsolationMode,
                                    Name = pAss.Name,
                                    Id = pAss.Id
                                }
                            }
                        };

            _log.LogInformation("Plugin steps obtained...");
            return [.. steps];
        }

        private async Task<pluginpackage> RegisterNuGetPackage(NugetAssemblyPackage packageOverview, Solution solution, CancellationToken ct = default)
        {
            _log.LogInformation("Registering Nuget package {name} with version {version}", packageOverview.Name, packageOverview.Version);

            var nugetPackage = _ctx.CreateQuery<pluginpackage>().Where(p => p.name == $"{solution.publisher_solution.CustomizationPrefix}_{packageOverview.Name}").Select(x => new pluginpackage()
            {
                Id = x.Id,
                name = x.name,
                Version = x.Version
            }).FirstOrDefault();

            nugetPackage ??= new pluginpackage();

            nugetPackage.Content = packageOverview.Base64Content;
            nugetPackage.name = $"{solution.publisher_solution.CustomizationPrefix}_{packageOverview.Name}";
            nugetPackage.Version = packageOverview.Version;

            if (nugetPackage.Id == Guid.Empty)
            {
                packageOverview.ProcessingAction = ProcessingAction.Create;
                packageOverview.Assemblies.ForEach(a =>
                {
                    a.RegisterableClasses.ForEach(c =>
                    {
                        c.ProcessingAction = ProcessingAction.Create;
                    });
                });

                nugetPackage.Id = await _serviceClient.CreateAsync(nugetPackage, ct).ConfigureAwait(false);
            }
            else
            {
                packageOverview.ProcessingAction = ProcessingAction.Update;

                await MarkPluginTypesAndRemoveOrphans(nugetPackage, packageOverview, ct).ConfigureAwait(false);

                await _serviceClient.UpdateAsync(nugetPackage, ct).ConfigureAwait(false);
            }
            packageOverview.PackageUploaded = true;

            await AddPluginPackageToSolution(solution.UniqueName, packageOverview.EntityTypeCode, nugetPackage, ct).ConfigureAwait(false);

            return nugetPackage;
        }

        private async Task MarkPluginTypesAndRemoveOrphans(pluginpackage nugetPackage, NugetAssemblyPackage package, CancellationToken ct = default)
        {
            _log.LogInformation("Marking Plugin Types for Nuget package {name} v{version}", package.Name, package.Version);
            var existingPluginTypes = GetExistingPluginTypes(nugetPackage, ct);

            var orphanedPluginTypes = existingPluginTypes.Where(x =>
            {
                var assembly = package.Assemblies.FirstOrDefault(a => a.AssemblyName == x.pluginassembly_plugintype.Name);
                if (assembly == null)
                {
                    return true;
                }
                return !assembly.RegisterableClasses.Any(c => c.ClassType.FullName == x.TypeName);
            }).ToList();

            _log.LogInformation("Found {count} orphaned Plugin Types", orphanedPluginTypes.Count);

            foreach (var assembly in package.Assemblies)
            {
                foreach (var registerableClass in assembly.RegisterableClasses)
                {
                    var typePresent = existingPluginTypes.Any(x => x.TypeName == registerableClass.ClassType.FullName && x.pluginassembly_plugintype.Name == assembly.AssemblyName);
                    registerableClass.ProcessingAction = typePresent ? ProcessingAction.Update : ProcessingAction.Create;
                }
            }

            if (orphanedPluginTypes.Count == 0)
            {
                return;
            }

            var customApisToDelink = await GetCustomAPIsToDelink(orphanedPluginTypes).ConfigureAwait(false);

            if (customApisToDelink.Count != 0)
            {
                _log.LogInformation("These custom APIs will have unlinked code logic implementation removed:\n{apis}", string.Join("\n", customApisToDelink.Select(x => x.Name)));
            }

            foreach (var item in customApisToDelink)
            {
                _log.LogTrace("Unlinking Custom API {name}", item.Name);
                await _serviceClient.UpdateAsync(item, ct).ConfigureAwait(false);
            }

            foreach (var item in orphanedPluginTypes)
            {
                _log.LogTrace("Unlinking Plugin Type {name}", item.TypeName);
                await _serviceClient.DeleteAsync(item.LogicalName, item.Id, ct).ConfigureAwait(false);
            }
        }

        private async Task<List<CustomAPI>> GetCustomAPIsToDelink(IEnumerable<PluginType> orphanedPluginTypes)
        {
            var query = new QueryExpression(CustomAPI.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(CustomAPI.Fields.Id, CustomAPI.Fields.Name),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression(CustomAPI.Fields.PluginTypeId, ConditionOperator.In, orphanedPluginTypes.Select(x => x.pluginassembly_plugintype.Id).ToList())
                    }
                }
            };

            var apis = await _serviceClient.RetrieveMultipleAsync(query).ConfigureAwait(false);

            if (!apis.Entities.Any())
            {
                return [];
            }

            return apis.Entities.Select(x => new CustomAPI()
            {
                Id = x.Id,
                Name = x.GetAttributeValue<string>(CustomAPI.Fields.Name),
                plugintype_customapi = null
            }).ToList();
        }

        private List<PluginType> GetExistingPluginTypes(pluginpackage nugetPackage, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _log.LogInformation("Obtaining plugin types...");

            var existingPluginTypes = (from pType in _ctx.CreateQuery<PluginType>()
                                       join pAss in _ctx.CreateQuery<PluginAssembly>()
                                         on pType.PluginAssemblyId.Id equals pAss.Id
                                       where pAss.PackageId.Id == nugetPackage.Id
                                       select new PluginType()
                                       {
                                           Id = pType.Id,
                                           TypeName = pType.TypeName,
                                           pluginassembly_plugintype = new PluginAssembly()
                                           {
                                               Name = pAss.Name,
                                               Id = pAss.Id
                                           }
                                       }).ToList();

            _log.LogInformation("Plugin types obtained...");
            return existingPluginTypes;
        }

        private int GetPluginPackageTypeCode()
        {
            var code = _ctx.CreateQuery<Entity_Ent>().Where(e => e.PhysicalName == pluginpackage.EntityLogicalName).Select(e => e.ObjectTypeCode).FirstOrDefault();

            return code ?? throw new Exception("Unsupported environment. Your environment does not have support for Dependent Assemblies.");
        }

        private async Task AddPluginPackageToSolution(string solutionName, int? nugetPackageTypeCode, pluginpackage nugetPackage, CancellationToken ct)
        {
            var req = new AddSolutionComponentRequest()
            {
                ComponentId = nugetPackage.Id,
                SolutionUniqueName = solutionName,
                AddRequiredComponents = true,
                DoNotIncludeSubcomponents = false,
                ComponentType = nugetPackageTypeCode.Value
            };

            await _serviceClient.ExecuteAsync(req, ct).ConfigureAwait(false);
        }

        private async Task AddPluginStepToSolution(string solutionName, Guid componentId, CancellationToken ct)
        {
            _log.LogInformation("Adding plugin step to solution {solution}", solutionName);
            var req = new AddSolutionComponentRequest()
            {
                ComponentId = componentId,
                SolutionUniqueName = solutionName,
                AddRequiredComponents = true,
                ComponentType = (int)componenttype.SDKMessageProcessingStep
            };

            await _serviceClient.ExecuteAsync(req, ct).ConfigureAwait(false);
            _log.LogInformation("Plugin step added to solution {solution}", solutionName);
        }
    }
}
