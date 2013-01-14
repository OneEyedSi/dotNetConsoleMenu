///////////////////////////////////////////////////////////////////////////////////////////////////
// Project      -   Datacom Building Blocks
// General      -   Classes that may be used by multiple projects in multiple solutions. 
//					Higher-level classes than those in the Utilities assemblies.
//
// File Name    -   CustomConfigurationCore.cs
// Description  -   Handler classes for processing custom elements in a config file.  These 
//					handlers define the schemas of the custom elements.
//
// Notes        -   The custom sections in the config file that contain these custom elements must 
//					be defined in the client application.
//
// $History: CustomConfigurationCore.cs $
// 
// *****************  Version 6  *****************
// User: Simone       Date: 5/05/09    Time: 1:32p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// MINOR: BaseConfigurationSection.SelectedMode property made virtual so
// it can be overridden in derived classes.
// 
// *****************  Version 5  *****************
// User: Simone       Date: 11/03/09   Time: 1:27p
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// SystemCoreConfiguration, SystemCoreSettingsCollection classes deleted.
// These will be defined in the client applications.
// 
// *****************  Version 4  *****************
// User: Simone       Date: 10/03/09   Time: 10:20a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// MINOR: "Core" added to names of non-base classes.
// 
// *****************  Version 3  *****************
// User: Simone       Date: 10/03/09   Time: 9:45a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// MINOR: Remove "partial" keyword from 2 class definitions - legacy of
// the original code this was copied from.
// 
// *****************  Version 2  *****************
// User: Simone       Date: 10/03/09   Time: 8:50a
// Updated in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Namespace changed from Datacom.BuildingBlocks to
// Utilities.BuildingBlocks.
// 
// *****************  Version 1  *****************
// User: Simone       Date: 10/03/09   Time: 8:43a
// Created in $/UtilitiesClassLibrary_DENG/Utilities.BuildingBlocks
// Handler classes for processing custom sections in a config file.  These
// handlers define the schemas of the custom sections.
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Configuration;
using Utilities.Miscellaneous;

namespace Utilities.BuildingBlocks
{
	#region Enums used in Config Files ************************************************************

	/// <summary>
	/// Indicates the mode the system should operate in, eg Test or Live.
	/// </summary>
	public enum OperatingMode
	{
		Invalid = 0,
		Dev,
		Test,
		Live
	}

	/// <summary>
	/// Indicates which protocol, such as HTTP-POST or SOAP, is used when calling a web method.
	/// </summary>
	public enum WebServiceProtocol
	{
		Default = 0,
		HttpGet,
		HttpPost,
		Soap
	}

	#endregion

	#region Class to Define System Settings Elements **********************************************

	/// <summary>
	/// Represents a custom system settings element in a config file.
	/// </summary>
	/// <remarks>Must include DefaultValue parameter of the ConfigurationProperty for elements 
	/// with a MinLength > 0.</remarks>
	public class SystemCoreSettings : BaseCollectionSettingsElement
	{
		public SystemCoreSettings()
			: base()
		{
		}

		public SystemCoreSettings(OperatingMode mode)
			: base(mode)
		{
		}

		/// <summary>
		/// Polling interval, in seconds, for polling the system for new messages.
		/// </summary>
		[ConfigurationProperty("pollingInterval", DefaultValue = 120, IsRequired = false)]
		[IntegerValidator(MinValue = 1)]
		public int PollingInterval
		{
			get
			{ return (int)this["pollingInterval"]; }
			set
			{ this["pollingInterval"] = value; }
		}

		/// <summary>
		/// Timeout, in seconds, when attempting to connect to a system.
		/// </summary>
		[ConfigurationProperty("timeout", DefaultValue = 30, IsRequired = false)]
		[IntegerValidator(MinValue = 5)]
		public int Timeout
		{
			get
			{ return (int)this["timeout"]; }
			set
			{ this["timeout"] = value; }
		}

		/// <summary>
		/// Settings to define the connection to a database.
		/// </summary>
		[ConfigurationProperty("database", IsRequired = false)]
		public DatabaseCoreSettings Database
		{
			get
			{ return (DatabaseCoreSettings)this["database"]; }
			set
			{ this["database"] = value; }
		}

		/// <summary>
		/// Settings to define the connection to a web service.
		/// </summary>
		[ConfigurationProperty("webService", IsRequired = false)]
		public WebServiceCoreSettings WebService
		{
			get
			{ return (WebServiceCoreSettings)this["webService"]; }
			set
			{ this["webService"] = value; }
		}
	}

	#endregion

	#region Class to Define Database Settings Elements ********************************************

	/// <summary>
	/// Represents a custom database settings element in a config file.
	/// </summary>
	/// <remarks>Must include DefaultValue parameter of the ConfigurationProperty for elements 
	/// with a MinLength > 0.</remarks>
	public class DatabaseCoreSettings : BaseSettingsElement
	{
		public DatabaseCoreSettings()
			: base()
		{
		}

		public DatabaseCoreSettings(string databaseServer, string database,
			string databaseLogin, string databasePassword)
			: base()
		{
			this.DatabaseServer = databaseServer;
			this.Database = database;
			this.DatabaseLogin = databaseLogin;
			this.DatabasePassword = databasePassword;
		}

		/// <summary>
		/// Name of SQL Server instance that staging database is on.
		/// </summary>
		/// <remarks>InvalidCharacters: Do not include "\" since it may be used in SQL Server 
		/// instance names (eg DSLCH1\SQL2000).</remarks>
		[ConfigurationProperty("databaseServer", DefaultValue = "DSLCH10", IsRequired = true)]
		[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|", MinLength = 1, MaxLength = 60)]
		public string DatabaseServer
		{
			get
			{ return (string)this["databaseServer"]; }
			set
			{ this["databaseServer"] = value; }
		}

		[ConfigurationProperty("database", DefaultValue = "AllianceInterface", IsRequired = true)]
		[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
		public string Database
		{
			get
			{ return (string)this["database"]; }
			set
			{ this["database"] = value; }
		}

		[ConfigurationProperty("databaseLogin", DefaultValue = "xxx", IsRequired = true)]
		[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
		public string DatabaseLogin
		{
			get
			{ return (string)this["databaseLogin"]; }
			set
			{ this["databaseLogin"] = value; }
		}

		/// <summary>
		/// Password required to log into database.
		/// </summary>
		/// <remarks>MaxLength must be huge as encrypted passwords can be several hundred characters long.  
		/// Doesn't have any invalid characters as passwords may include characters such as "@", "$".</remarks>
		[ConfigurationProperty("databasePassword", DefaultValue = "", IsRequired = false)]
		[StringValidator(MinLength = 0, MaxLength = 500)]
		public string DatabasePassword
		{
			get
			{
				// Descrypt the password, if required.
				string passwordFromConfig = (string)this["databasePassword"];
				return this.GetPassword(passwordFromConfig);
			}
			set
			{ this["databasePassword"] = value; }
		}
	}

	#endregion

	#region Class to Define Web Service Settings Elements *****************************************

	/// <summary>
	/// Represents a custom webService settings element in a config file.
	/// </summary>
	/// <remarks>Must include DefaultValue parameter of the ConfigurationProperty for elements 
	/// with a MinLength > 0.</remarks>
	public class WebServiceCoreSettings : BaseSettingsElement
	{
		public WebServiceCoreSettings()
			: base()
		{
		}

		public WebServiceCoreSettings(string url)
			: this(url, string.Empty, string.Empty)
		{
		}

		public WebServiceCoreSettings(string url, string login, string password)
			: base()
		{
			this.Url = url;
			this.Login = login;
			this.Password = password;
		}

		/// <summary>
		/// URL of web service to connect to.
		/// </summary>
		/// <remarks>InvalidCharacters: Do not include "/", "?" or "&" since they may be used in 
		/// the URL.</remarks>
		[ConfigurationProperty("url", DefaultValue = "", IsRequired = true)]
		[StringValidator(InvalidCharacters = "~!@#$%^*()[]{};'\"|\\", MinLength = 0, MaxLength = 200)]
		public string Url
		{
			get
			{ return (string)this["url"]; }
			set
			{ this["url"] = value; }
		}

		/// <summary>
		/// Login name for logging into the web service.
		/// </summary>
		/// <remarks>InvalidCharacters: Do not include "\\" (escaped \) since "\" may be used in 
		/// logins of the form: domain\login.</remarks>
		[ConfigurationProperty("login", DefaultValue = "", IsRequired = false)]
		[StringValidator(InvalidCharacters = "~!?@#$%^&*()[]{}/;'\"|", MinLength = 0, MaxLength = 100)]
		public string Login
		{
			get
			{ return (string)this["login"]; }
			set
			{ this["login"] = value; }
		}

		/// <summary>
		/// Password for logging into the web service.
		/// </summary>
		/// <remarks>MaxLength must be huge as encrypted passwords can be several hundred characters long.  
		/// Doesn't have any invalid characters as passwords may include characters such as "@", "$".</remarks>
		[ConfigurationProperty("password", DefaultValue = "", IsRequired = false)]
		[StringValidator(MinLength = 0, MaxLength = 500)]
		public string Password
		{
			get
			{
				// Descrypt the password, if required.
				string passwordFromConfig = (string)this["password"];
				return this.GetPassword(passwordFromConfig);
			}
			set
			{ this["password"] = value; }
		}

		/// <summary>
		/// Protocol used when calling the web service.
		/// </summary>
		[ConfigurationProperty("protocol", DefaultValue = WebServiceProtocol.Soap,
			IsRequired = false)]
		public WebServiceProtocol Protocol
		{
			get
			{
				return (WebServiceProtocol)this["protocol"];
			}
			set
			{ this["protocol"] = value; }
		}
	}

	#endregion

	#region Base Class for Config File Custom Sections ********************************************

	/// <summary>
	/// Base class that custom configuration sections are derived from.
	/// </summary>
	public abstract class BaseConfigurationSection : ConfigurationSection
	{
		/// <summary>
		/// Determines which settings will be read from the config file.
		/// </summary>
		[ConfigurationProperty("selectedMode", DefaultValue = OperatingMode.Invalid,
			IsRequired = true)]
		public virtual OperatingMode SelectedMode
		{
			get
			{
				return (OperatingMode)this["selectedMode"];
			}
			set
			{ this["selectedMode"] = value; }
		}
	}

	#endregion

	#region Base Class for Config File Custom Collections *****************************************

	/// <summary>
	/// Represents a collection of custom settings elements in a config file.
	/// </summary>
	public class CustomSettingsCollection<T> : ConfigurationElementCollection
		where T : BaseCollectionSettingsElement, new()
	{
		private OperatingMode _selectedMode = OperatingMode.Invalid;
		private string _elementName = string.Empty;
		private string _configParentSectionName = string.Empty;

		public CustomSettingsCollection()
			: this(OperatingMode.Invalid)
		{
		}

		public CustomSettingsCollection(OperatingMode selectedMode)
		{
			_selectedMode = selectedMode;
			//T newElement = new T();
			//Add(newElement);
		}

		/// <summary>
		/// Operating mode that is selected in the config file, eg Test, Live.
		/// </summary>
		public OperatingMode SelectedMode
		{
			get { return _selectedMode; }
			set { _selectedMode = value; }
		}

		/// <summary>
		/// Gets the settings specified by the SelectedMode property.
		/// </summary>
		public T SelectedSettings
		{
			get { return (T)BaseGet(_selectedMode); }
		}

		/// <summary>
		/// The element name used for the collection in the config file.  
		/// </summary>
		/// <remarks>WARNING: ElementName must be set before trying to read from the config file.
		/// Read-only property in the base class - cannot add a set accessor.
		/// Only needed for CollectionType = BasicMap.  
		/// For CollectionType = AddRemoveClearMap the AddItemName, ClearItemName, 
		/// RemoveItemName would be specified in the ConfigurationCollection attribute of the 
		/// SystemSettings property of the SystemConfiguration class, above.
		/// </remarks>
		protected override string ElementName
		{
			get { return _elementName; }
		}

		/// <summary>
		/// Sets the ElementName property.
		/// </summary>
		/// <remarks>Added as the ElementName property is read-only.</remarks>
		public void SetElementName(string elementName)
		{
			_elementName = elementName;
		}

		/// <summary>
		/// The Tag name of the top-level section in the config file that contains this 
		/// collection.
		/// </summary>
		public string ConfigParentSectionName
		{
			get { return _configParentSectionName; }
			set
			{
				_configParentSectionName = value;
				foreach (T element in this)
				{
					element.ConfigParentSectionName = _configParentSectionName;
				}
			}
		}

		/// <summary>
		/// Specifies the format of the collection and which directives can be included within it.  
		/// eg AddRemoveClearMap indicates that the collection can contain add, remove and clear 
		/// directives only.
		/// </summary>
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.BasicMap;
			}
		}

		/// <summary>
		/// Creates a new element but does not add it to the collection.  Used in conjunction 
		/// with Add.
		/// </summary>
		/// <returns></returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new T();
		}

		/// <summary>
		/// Given an element in the collection, returns the key of that element.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		protected override Object GetElementKey(ConfigurationElement element)
		{
			return ((T)element).Mode;
		}

		/// <summary>
		/// Numeric indexer.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public T this[int index]
		{
			get
			{
				return (T)BaseGet(index);
			}
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}

		/// <summary>
		/// Indexer based on element key.
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		public T this[OperatingMode mode]
		{
			get
			{
				return (T)BaseGet(mode);
			}
		}

		/// <summary>
		/// Returns the numeric index of a given element in the collection.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public int IndexOf(T element)
		{
			return BaseIndexOf(element);
		}

		/// <summary>
		/// Adds a new element to the collection.  Used in conjunction with CreateNewElement.
		/// </summary>
		/// <param name="element"></param>
		public void Add(T element)
		{
			BaseAdd(element);
		}
		
		/// <summary>
		/// Adds a new element to the collection.  Used in conjunction with CreateNewElement.
		/// </summary>
		/// <param name="element"></param>
		protected override void BaseAdd(ConfigurationElement element)
		{
			BaseAdd(element, false);
		}

		/// <summary>
		/// Removes a given element from the collection.
		/// </summary>
		/// <param name="element"></param>
		public void Remove(T element)
		{
			if (BaseIndexOf(element) >= 0)
				BaseRemove(element.Mode);
		}

		/// <summary>
		/// Removes the element at the given index from the collection.
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		/// <summary>
		/// Removes the element with the given key from the collection (the mode is the collection key).
		/// </summary>
		/// <param name="mode"></param>
		public void Remove(OperatingMode mode)
		{
			BaseRemove(mode);
		}

		/// <summary>
		/// removes all elements from the collection.
		/// </summary>
		public void Clear()
		{
			BaseClear();
		}
	}

	#endregion

	#region Base Class for Custom Elements that are members of a Collection ***********************

	/// <summary>
	/// Abstract base class that custom config elements that are members of a collection are 
	/// derived from.
	/// </summary>
	public abstract class BaseCollectionSettingsElement : BaseSettingsElement
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public BaseCollectionSettingsElement()
		{
		}

		public BaseCollectionSettingsElement(OperatingMode mode)
			: base()
		{
			this.Mode = mode;
		}

		/// <summary>
		/// Mode the system is operating in, eg Test, Live.
		/// </summary>
		[ConfigurationProperty("mode", DefaultValue = OperatingMode.Invalid, IsRequired = true,
		  IsKey = true)]
		//[ConfigurationProperty("mode", IsRequired = true,
		//    IsKey = true)]
		public OperatingMode Mode
		{
			get
			{
				return (OperatingMode)this["mode"];
			}
			set
			{ this["mode"] = value; }
		}
	}

	#endregion

	#region Base Class for Config File Custom Elements ********************************************

	/// <summary>
	/// Abstract base class that custom config elements are derived from.
	/// </summary>
	/// <remarks>For elements that are members of a collection, needing a "mode" attribute, 
	/// derive from BaseCollectionSettingsElement rather than BaseSettingsElement.</remarks>
	public abstract class BaseSettingsElement : ConfigurationElement
	{
		private string _configParentSectionName = string.Empty;

		/// <summary>
		/// Constructor.
		/// </summary>
		public BaseSettingsElement()
		{
		}

		/// <summary>
		/// The Tag name of the top-level section in the config file that contains this 
		/// element.
		/// </summary>
		public string ConfigParentSectionName
		{
			get { return _configParentSectionName; }
			set { _configParentSectionName = value; }
		}

		/// <summary>
		/// Returns a password in clear text, regardless of whether it was encrypted in the config file or not.
		/// </summary>
		/// <param name="passwordFromConfig"></param>
		/// <returns>Password in clear.</returns>
		/// <remarks>Assumption: Passwords longer than 50 characters are encrypted; 
		/// those that are no longer than 50 characters are not.</remarks>
		protected string GetPassword(string passwordFromConfig)
		{
			string modifiedPassword = string.Empty;

			if (passwordFromConfig.Length > 50)
			{
				Encryptor encryptor = new Encryptor();
				modifiedPassword = encryptor.DecryptString(passwordFromConfig);
			}
			else
			{
				modifiedPassword = passwordFromConfig;
			}

			return modifiedPassword;
		}
	}

	#endregion
}
