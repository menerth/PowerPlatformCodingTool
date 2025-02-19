#pragma warning disable CS1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PPCT.Models.Dataverse
{
	
	
	/// <summary>
	/// Authentication Type for the Web sources like AzureWebApp, for example 0=BasicAuth
	/// </summary>
	[System.Runtime.Serialization.DataContractAttribute()]
	public enum pluginassembly_authtype
	{
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		BasicAuth = 0,
	}
	
	/// <summary>
	/// Information about how the plugin assembly is to be isolated at execution time; None / Sandboxed / External.
	/// </summary>
	[System.Runtime.Serialization.DataContractAttribute()]
	public enum pluginassembly_isolationmode
	{
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		None = 1,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		Sandbox = 2,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		External = 3,
	}
	
	/// <summary>
	/// Location of the assembly, for example 0=database, 1=on-disk, 2=Normal, 3=AzureWebApp.
	/// </summary>
	[System.Runtime.Serialization.DataContractAttribute()]
	public enum pluginassembly_sourcetype
	{
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		Database = 0,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		Disk = 1,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		Normal = 2,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		AzureWebApp = 3,
		
		[System.Runtime.Serialization.EnumMemberAttribute()]
		FileStore = 4,
	}
	
	/// <summary>
	/// Assembly that contains one or more plug-in types.
	/// </summary>
	[System.Runtime.Serialization.DataContractAttribute()]
	[Microsoft.Xrm.Sdk.Client.EntityLogicalNameAttribute("pluginassembly")]
	public partial class PluginAssembly : Microsoft.Xrm.Sdk.Entity
	{
		
		/// <summary>
		/// Available fields, a the time of codegen, for the pluginassembly entity
		/// </summary>
		public partial class Fields
		{
			public const string AuthType = "authtype";
			public const string ComponentState = "componentstate";
			public const string Content = "content";
			public const string CreatedBy = "createdby";
			public const string CreatedOn = "createdon";
			public const string CreatedOnBehalfBy = "createdonbehalfby";
			public const string Culture = "culture";
			public const string CustomizationLevel = "customizationlevel";
			public const string Description = "description";
			public const string IntroducedVersion = "introducedversion";
			public const string IsCustomizable = "iscustomizable";
			public const string IsHidden = "ishidden";
			public const string IsManaged = "ismanaged";
			public const string IsolationMode = "isolationmode";
			public const string IsPasswordSet = "ispasswordset";
			public const string Major = "major";
			public const string ManagedIdentityId = "managedidentityid";
			public const string Minor = "minor";
			public const string ModifiedBy = "modifiedby";
			public const string ModifiedOn = "modifiedon";
			public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
			public const string Name = "name";
			public const string OrganizationId = "organizationid";
			public const string OverwriteTime = "overwritetime";
			public const string PackageId = "packageid";
			public const string Password = "password";
			public const string Path = "path";
			public const string PluginAssemblyId = "pluginassemblyid";
			public const string Id = "pluginassemblyid";
			public const string PluginAssemblyIdUnique = "pluginassemblyidunique";
			public const string PublicKeyToken = "publickeytoken";
			public const string SolutionId = "solutionid";
			public const string SourceHash = "sourcehash";
			public const string SourceType = "sourcetype";
			public const string Url = "url";
			public const string UserName = "username";
			public const string Version = "version";
			public const string VersionNumber = "versionnumber";
			public const string pluginassembly_plugintype = "pluginassembly_plugintype";
			public const string pluginpackage_pluginassembly = "pluginpackage_pluginassembly";
		}
		
		/// <summary>
		/// Default Constructor.
		/// </summary>
		public PluginAssembly() : 
				base(EntityLogicalName)
		{
		}
		
		public const string EntityLogicalName = "pluginassembly";
		
		public const string EntityLogicalCollectionName = "pluginassemblies";
		
		public const string EntitySetName = "pluginassemblies";
		
		/// <summary>
		/// Specifies mode of authentication with web sources like WebApp
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("authtype")]
		public virtual pluginassembly_authtype? AuthType
		{
			get
			{
				return ((pluginassembly_authtype?)(EntityOptionSetEnum.GetEnum(this, "authtype")));
			}
			set
			{
				this.SetAttributeValue("authtype", value.HasValue ? new Microsoft.Xrm.Sdk.OptionSetValue((int)value) : null);
			}
		}
		
		/// <summary>
		/// For internal use only.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("componentstate")]
		public virtual componentstate? ComponentState
		{
			get
			{
				return ((componentstate?)(EntityOptionSetEnum.GetEnum(this, "componentstate")));
			}
		}
		
		/// <summary>
		/// Bytes of the assembly, in Base64 format.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("content")]
		public string Content
		{
			get
			{
				return this.GetAttributeValue<string>("content");
			}
			set
			{
				this.SetAttributeValue("content", value);
			}
		}
		
		/// <summary>
		/// Unique identifier of the user who created the plug-in assembly.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdby")]
		public Microsoft.Xrm.Sdk.EntityReference CreatedBy
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("createdby");
			}
		}
		
		/// <summary>
		/// Date and time when the plug-in assembly was created.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdon")]
		public System.Nullable<System.DateTime> CreatedOn
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.DateTime>>("createdon");
			}
		}
		
		/// <summary>
		/// Unique identifier of the delegate user who created the pluginassembly.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("createdonbehalfby")]
		public Microsoft.Xrm.Sdk.EntityReference CreatedOnBehalfBy
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("createdonbehalfby");
			}
		}
		
		/// <summary>
		/// Culture code for the plug-in assembly.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("culture")]
		public string Culture
		{
			get
			{
				return this.GetAttributeValue<string>("culture");
			}
			set
			{
				this.SetAttributeValue("culture", value);
			}
		}
		
		/// <summary>
		/// Customization Level.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("customizationlevel")]
		public System.Nullable<int> CustomizationLevel
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("customizationlevel");
			}
		}
		
		/// <summary>
		/// Description of the plug-in assembly.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("description")]
		public string Description
		{
			get
			{
				return this.GetAttributeValue<string>("description");
			}
			set
			{
				this.SetAttributeValue("description", value);
			}
		}
		
		/// <summary>
		/// Version in which the form is introduced.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("introducedversion")]
		public string IntroducedVersion
		{
			get
			{
				return this.GetAttributeValue<string>("introducedversion");
			}
			set
			{
				this.SetAttributeValue("introducedversion", value);
			}
		}
		
		/// <summary>
		/// Information that specifies whether this component can be customized.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("iscustomizable")]
		public Microsoft.Xrm.Sdk.BooleanManagedProperty IsCustomizable
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.BooleanManagedProperty>("iscustomizable");
			}
			set
			{
				this.SetAttributeValue("iscustomizable", value);
			}
		}
		
		/// <summary>
		/// Information that specifies whether this component should be hidden.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("ishidden")]
		public Microsoft.Xrm.Sdk.BooleanManagedProperty IsHidden
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.BooleanManagedProperty>("ishidden");
			}
			set
			{
				this.SetAttributeValue("ishidden", value);
			}
		}
		
		/// <summary>
		/// Information that specifies whether this component is managed.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("ismanaged")]
		public System.Nullable<bool> IsManaged
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<bool>>("ismanaged");
			}
		}
		
		/// <summary>
		/// Information about how the plugin assembly is to be isolated at execution time; None / Sandboxed.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("isolationmode")]
		public virtual pluginassembly_isolationmode? IsolationMode
		{
			get
			{
				return ((pluginassembly_isolationmode?)(EntityOptionSetEnum.GetEnum(this, "isolationmode")));
			}
			set
			{
				this.SetAttributeValue("isolationmode", value.HasValue ? new Microsoft.Xrm.Sdk.OptionSetValue((int)value) : null);
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("ispasswordset")]
		public System.Nullable<bool> IsPasswordSet
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<bool>>("ispasswordset");
			}
		}
		
		/// <summary>
		/// Major of the assembly version.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("major")]
		public System.Nullable<int> Major
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("major");
			}
		}
		
		/// <summary>
		/// Unique identifier for managedidentity associated with pluginassembly.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("managedidentityid")]
		public Microsoft.Xrm.Sdk.EntityReference ManagedIdentityId
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("managedidentityid");
			}
			set
			{
				this.SetAttributeValue("managedidentityid", value);
			}
		}
		
		/// <summary>
		/// Minor of the assembly version.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("minor")]
		public System.Nullable<int> Minor
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<int>>("minor");
			}
		}
		
		/// <summary>
		/// Unique identifier of the user who last modified the plug-in assembly.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedby")]
		public Microsoft.Xrm.Sdk.EntityReference ModifiedBy
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("modifiedby");
			}
		}
		
		/// <summary>
		/// Date and time when the plug-in assembly was last modified.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedon")]
		public System.Nullable<System.DateTime> ModifiedOn
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.DateTime>>("modifiedon");
			}
		}
		
		/// <summary>
		/// Unique identifier of the delegate user who last modified the pluginassembly.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("modifiedonbehalfby")]
		public Microsoft.Xrm.Sdk.EntityReference ModifiedOnBehalfBy
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("modifiedonbehalfby");
			}
		}
		
		/// <summary>
		/// Name of the plug-in assembly.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("name")]
		public string Name
		{
			get
			{
				return this.GetAttributeValue<string>("name");
			}
			set
			{
				this.SetAttributeValue("name", value);
			}
		}
		
		/// <summary>
		/// Unique identifier of the organization with which the plug-in assembly is associated.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("organizationid")]
		public Microsoft.Xrm.Sdk.EntityReference OrganizationId
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("organizationid");
			}
		}
		
		/// <summary>
		/// For internal use only.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("overwritetime")]
		public System.Nullable<System.DateTime> OverwriteTime
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.DateTime>>("overwritetime");
			}
		}
		
		/// <summary>
		/// Unique identifier for Plugin Package associated with Plug-in Assembly.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("packageid")]
		public Microsoft.Xrm.Sdk.EntityReference PackageId
		{
			get
			{
				return this.GetAttributeValue<Microsoft.Xrm.Sdk.EntityReference>("packageid");
			}
			set
			{
				this.SetAttributeValue("packageid", value);
			}
		}
		
		/// <summary>
		/// User Password
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("password")]
		public string Password
		{
			get
			{
				return this.GetAttributeValue<string>("password");
			}
			set
			{
				this.SetAttributeValue("password", value);
			}
		}
		
		/// <summary>
		/// File name of the plug-in assembly. Used when the source type is set to 1.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("path")]
		public string Path
		{
			get
			{
				return this.GetAttributeValue<string>("path");
			}
			set
			{
				this.SetAttributeValue("path", value);
			}
		}
		
		/// <summary>
		/// Unique identifier of the plug-in assembly.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("pluginassemblyid")]
		public System.Nullable<System.Guid> PluginAssemblyId
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("pluginassemblyid");
			}
			set
			{
				this.SetAttributeValue("pluginassemblyid", value);
				if (value.HasValue)
				{
					base.Id = value.Value;
				}
				else
				{
					base.Id = System.Guid.Empty;
				}
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("pluginassemblyid")]
		public override System.Guid Id
		{
			get
			{
				return base.Id;
			}
			set
			{
				this.PluginAssemblyId = value;
			}
		}
		
		/// <summary>
		/// Unique identifier of the plug-in assembly.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("pluginassemblyidunique")]
		public System.Nullable<System.Guid> PluginAssemblyIdUnique
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("pluginassemblyidunique");
			}
		}
		
		/// <summary>
		/// Public key token of the assembly. This value can be obtained from the assembly by using reflection.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("publickeytoken")]
		public string PublicKeyToken
		{
			get
			{
				return this.GetAttributeValue<string>("publickeytoken");
			}
			set
			{
				this.SetAttributeValue("publickeytoken", value);
			}
		}
		
		/// <summary>
		/// Unique identifier of the associated solution.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("solutionid")]
		public System.Nullable<System.Guid> SolutionId
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<System.Guid>>("solutionid");
			}
		}
		
		/// <summary>
		/// Hash of the source of the assembly.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("sourcehash")]
		public string SourceHash
		{
			get
			{
				return this.GetAttributeValue<string>("sourcehash");
			}
			set
			{
				this.SetAttributeValue("sourcehash", value);
			}
		}
		
		/// <summary>
		/// Location of the assembly, for example 0=database, 1=on-disk.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("sourcetype")]
		public virtual pluginassembly_sourcetype? SourceType
		{
			get
			{
				return ((pluginassembly_sourcetype?)(EntityOptionSetEnum.GetEnum(this, "sourcetype")));
			}
			set
			{
				this.SetAttributeValue("sourcetype", value.HasValue ? new Microsoft.Xrm.Sdk.OptionSetValue((int)value) : null);
			}
		}
		
		/// <summary>
		/// Web Url
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("url")]
		public string Url
		{
			get
			{
				return this.GetAttributeValue<string>("url");
			}
			set
			{
				this.SetAttributeValue("url", value);
			}
		}
		
		/// <summary>
		/// User Name
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("username")]
		public string UserName
		{
			get
			{
				return this.GetAttributeValue<string>("username");
			}
			set
			{
				this.SetAttributeValue("username", value);
			}
		}
		
		/// <summary>
		/// Version number of the assembly. The value can be obtained from the assembly through reflection.
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("version")]
		public string Version
		{
			get
			{
				return this.GetAttributeValue<string>("version");
			}
			set
			{
				this.SetAttributeValue("version", value);
			}
		}
		
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("versionnumber")]
		public System.Nullable<long> VersionNumber
		{
			get
			{
				return this.GetAttributeValue<System.Nullable<long>>("versionnumber");
			}
		}
		
		/// <summary>
		/// 1:N pluginassembly_plugintype
		/// </summary>
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("pluginassembly_plugintype")]
		public System.Collections.Generic.IEnumerable<PPCT.Models.Dataverse.PluginType> pluginassembly_plugintype
		{
			get
			{
				return this.GetRelatedEntities<PPCT.Models.Dataverse.PluginType>("pluginassembly_plugintype", null);
			}
			set
			{
				this.SetRelatedEntities<PPCT.Models.Dataverse.PluginType>("pluginassembly_plugintype", null, value);
			}
		}
		
		/// <summary>
		/// N:1 pluginpackage_pluginassembly
		/// </summary>
		[Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute("packageid")]
		[Microsoft.Xrm.Sdk.RelationshipSchemaNameAttribute("pluginpackage_pluginassembly")]
		public PPCT.Models.Dataverse.pluginpackage pluginpackage_pluginassembly
		{
			get
			{
				return this.GetRelatedEntity<PPCT.Models.Dataverse.pluginpackage>("pluginpackage_pluginassembly", null);
			}
			set
			{
				this.SetRelatedEntity<PPCT.Models.Dataverse.pluginpackage>("pluginpackage_pluginassembly", null, value);
			}
		}
	}
}
#pragma warning restore CS1591
