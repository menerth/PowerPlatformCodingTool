using System;

namespace PPCT.Components
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class DataverseRegistrationAttribute : Attribute
    {
        public DataverseRegistrationAttribute(
    string message)
        {
            Message = message;
            IsolationMode = IsolationModeEnum.Sandbox;
            RegistrationType = RegistrationTypeEnum.CustomApi;
        }

        public DataverseRegistrationAttribute(
            string message,
            string entityLogicalName,
            StageEnum stage,
            ExecutionModeEnum executionMode,
            string filteringAttributes,
            string stepName,
            int executionOrder,
            IsolationModeEnum isolationModel

            )
        {
            Message = message;
            EntityLogicalName = entityLogicalName;
            FilteringAttributes = filteringAttributes;
            Name = stepName;
            ExecutionOrder = executionOrder;
            Stage = stage;
            ExecutionMode = executionMode;
            IsolationMode = isolationModel;
            Offline = false;
            Server = true;
            RegistrationType = RegistrationTypeEnum.Plugin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">Message Name</param>
        /// <param name="entityLogicalName"></param>
        /// <param name="stage"></param>
        /// <param name="executionMode"></param>
        /// <param name="filteringAttributes">Comma separated list of attributes that will trigger this step. Leave empty string for all attributes.</param>
        /// <param name="stepName"></param>
        /// <param name="executionOrder"></param>
        /// <param name="isolationMode"></param>
        public DataverseRegistrationAttribute(
            MessageNameEnum message,
            string entityLogicalName,
            StageEnum stage,
            ExecutionModeEnum executionMode,
            string filteringAttributes,
            string stepName,
            int executionOrder,
            IsolationModeEnum isolationMode

            ) : this(message.ToString(), entityLogicalName, stage, executionMode, filteringAttributes, stepName, executionOrder, isolationMode)
        {

        }

        public DataverseRegistrationAttribute(
            string message,
            string entityLogicalName,
            StageEnum stage,
            ExecutionModeEnum executionMode,
            string filteringAttributes,
            string stepName,
            int executionOrder,
            IsolationModeEnum isolationMode,
            string id
            )
        {
            Message = message;
            EntityLogicalName = entityLogicalName;
            FilteringAttributes = filteringAttributes;
            Name = stepName;
            ExecutionOrder = executionOrder;
            Stage = stage;
            ExecutionMode = executionMode;
            IsolationMode = isolationMode;
            Offline = false;
            Server = true;
            RegistrationType = RegistrationTypeEnum.Plugin;
            Id = id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">Message Name</param>
        /// <param name="entityLogicalName"></param>
        /// <param name="stage"></param>
        /// <param name="executionMode"></param>
        /// <param name="filteringAttributes">Comma separated list of attributes that will trigger this step. Leave empty string for all attributes.</param>
        /// <param name="stepName"></param>
        /// <param name="executionOrder"></param>
        /// <param name="isolationMode"></param>
        public DataverseRegistrationAttribute(
            MessageNameEnum message,
            string entityLogicalName,
            StageEnum stage,
            ExecutionModeEnum executionMode,
            string filteringAttributes,
            string stepName,
            int executionOrder,
            IsolationModeEnum isolationMode,
            string id

            ) : this(message.ToString(), entityLogicalName, stage, executionMode, filteringAttributes, stepName, executionOrder, isolationMode, id)
        {

        }

        /// <summary>
        /// Create workflow activity registration
        /// </summary>
        /// <param name="name">Name of the Workflow Activity</param>
        /// <param name="friendlyName">Friendly name</param>
        /// <param name="description">Description</param>
        /// <param name="groupName">Group Name</param>
        public DataverseRegistrationAttribute(
            string name,
            string friendlyName,
            string description,
            string groupName,
            IsolationModeEnum isolationModel
            )
        {
            Name = name;
            FriendlyName = friendlyName;
            Description = description;
            GroupName = groupName;
            IsolationMode = isolationModel;
            RegistrationType = RegistrationTypeEnum.Workflow;
        }

        #region Named Properties
        public string Id { get; set; }
        public Guid StepId
        {
            get
            {
                return Guid.TryParse(Id, out Guid id) ? id : Guid.Empty;
            }
        }
        public string FriendlyName { get; set; }
        public string GroupName { get; set; }
        public string Image1Name { get; set; }
        public string Image1Attributes { get; set; }
        public string Image2Name { get; set; }
        public string Image2Attributes { get; set; }
        public string Description { get; set; }
        public bool DeleteAsyncOperation { get; set; }
        public string UnSecureConfiguration { get; set; }
        public string SecureConfiguration { get; set; }
        public bool Offline { get; set; }
        public bool Server { get; set; }
        public ImageTypeEnum Image1Type { get; set; }
        public ImageTypeEnum Image2Type { get; set; }
        public PluginStepOperationEnum? Action { get; set; }
        public RegistrationTypeEnum RegistrationType { get; set; }
        public ImpersonationTypeEnum? ExecuteAs { get; set; }
        #endregion

        #region Constructor Mandatory Properties
        public IsolationModeEnum IsolationMode { get; private set; }
        public string Message { get; private set; }
        public string EntityLogicalName { get; private set; }
        public string FilteringAttributes { get; private set; }
        public string Name { get; private set; }
        public int ExecutionOrder { get; private set; }
        public StageEnum? Stage { get; private set; }
        public ExecutionModeEnum ExecutionMode { get; private set; }
        #endregion
    }

    public enum ExecutionModeEnum
    {
        Asynchronous,
        Synchronous
    }
    public enum ImageTypeEnum
    {
        PreImage = 0,
        PostImage = 1,
        Both = 2
    }
    public enum IsolationModeEnum
    {
        None = 0,
        Sandbox = 1
    }
    public enum MessageNameEnum
    {
        AddItem,
        AddListMembers,
        AddMember,
        AddMembers,
        AddPrincipalToQueue,
        AddPrivileges,
        AddProductToKit,
        AddRecurrence,
        AddToQueue,
        AddUserToRecordTeam,
        ApplyRecordCreationAndUpdateRule,
        Assign,
        Associate,
        BackgroundSend,
        Book,
        CalculatePrice,
        Cancel,
        CheckIncoming,
        CheckPromote,
        Clone,
        CloneMobileOfflineProfile,
        CloneProduct,
        Close,
        CopyDynamicListToStatic,
        CopySystemForm,
        Create,
        CreateException,
        CreateInstance,
        CreateKnowledgeArticleTranslation,
        CreateKnowledgeArticleVersion,
        CreateMultiple,
        Delete,
        DeleteMultiple,
        DeleteOpenInstances,
        DeliverIncoming,
        DeliverPromote,
        Disassociate,
        Execute,
        ExecuteById,
        Export,
        GenerateSocialProfile,
        GetDefaultPriceLevel,
        GrantAccess,
        Import,
        LockInvoicePricing,
        LockSalesOrderPricing,
        Lose,
        Merge,
        ModifyAccess,
        PickFromQueue,
        Publish,
        PublishAll,
        PublishTheme,
        QualifyLead,
        Recalculate,
        ReleaseToQueue,
        RemoveFromQueue,
        RemoveItem,
        RemoveMember,
        RemoveMembers,
        RemovePrivilege,
        RemoveProductFromKit,
        RemoveRelated,
        RemoveUserFromRecordTeam,
        ReplacePrivileges,
        Reschedule,
        Retrieve,
        RetrieveExchangeRate,
        RetrieveFilteredForms,
        RetrieveMultiple,
        RetrievePersonalWall,
        RetrievePrincipalAccess,
        RetrieveRecordWall,
        RetrieveSharedPrincipalsAndAccess,
        RetrieveUnpublished,
        RetrieveUnpublishedMultiple,
        RetrieveUserQueues,
        RevokeAccess,
        RouteTo,
        Send,
        SendFromTemplate,
        SetLocLabels,
        SetRelated,
        SetState,
        TriggerServiceEndpointCheck,
        UnlockInvoicePricing,
        UnlockSalesOrderPricing,
        Update,
        UpdateMultiple,
        ValidateRecurrenceRule,
        Win
    }
    public enum PluginStepOperationEnum
    {
        Delete = 0,
        Deactivate = 1,
    }

    public enum StageEnum
    {
        PreValidation = 10,
        PreOperation = 20,
        InOperation = 30,
        PostOperation = 40
    }

    public enum RegistrationTypeEnum
    {
        Plugin,
        Workflow,
        CustomApi
    }

    public enum ImpersonationTypeEnum
    {
        Skip = 0,
        CallingUser = 1,
        System = 2
    }
}

