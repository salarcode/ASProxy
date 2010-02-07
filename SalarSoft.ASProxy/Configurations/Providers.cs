using System;
using System.Collections;
using System.Reflection;
using System.Xml;
using SalarSoft.ASProxy.BuiltIn;
using System.Web;
using System.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace SalarSoft.ASProxy
{
	/// <summary>
	/// Extensions Provider Loader
	/// </summary>
	public static class Providers
	{
		#region variables
		private static Hashtable _loadedProviders;
		private static List<ProviderInfo> _installedProviders;
		#endregion

		#region properties
		/// <summary>
		/// Implementation list
		/// </summary>
		internal static Hashtable LoadedProviders
		{
			get { return _loadedProviders; }
			set { _loadedProviders = value; }
		}

		/// <summary>
		/// All installed providers, includes disabled and failed to load providers
		/// </summary>
		internal static List<ProviderInfo> InstalledProviders
		{
			get
			{
				return _installedProviders;
			}
		}
		#endregion

		#region public methods
		static Providers()
		{
			_loadedProviders = new Hashtable();
			_installedProviders = new List<ProviderInfo>();

			if (Configurations.Providers.ProviderExtensionsEnabled)
				ReadInstalledProviders();
		}

		/// <summary>
		/// Creates an instance of requsted provider.
		/// If specified provider not find the default provider will be used.
		/// </summary>
		public static object GetProvider(ProviderType providerType)
		{
			Type provider = GetProviderType(providerType);

			if (provider == null)
				return GetBuiltinProvider(providerType);

			object result = InvokeDefaultCreateInstance(provider);
			if ((result is bool) && ((bool)result == false))
				return GetBuiltinProvider(providerType);

			return result;
		}

		/// <summary>
		/// Creates a default instance of requsted provider.
		/// </summary>
		public static object GetBuiltinProvider(ProviderType providerType)
		{
			switch (providerType)
			{
				case ProviderType.IEngine:
					return new ASProxyEngine();

				case ProviderType.IWebData:
					return new WebData();

				case ProviderType.ICredentialCache:
					return new ASProxyCredentialCache();

				case ProviderType.ILogSystem:
					return new LogSystem();

				case ProviderType.ICookieManager:
					return new CookieManager();

				case ProviderType.IHtmlProcessor:
					return new RegexHtmlProcessor();

				case ProviderType.ICssProcessor:
					return new CSSProcessor();

				case ProviderType.IJSProcessor:
					return new JSProcessor();

				case ProviderType.IUAC:
					return new UAC();
			}
			return null;
		}
		#endregion

		#region private methods

		private static string ProvidersLocation
		{
			get
			{
				return CurrentContext.MapAppPath(Consts.FilesConsts.Dir_Providers);
			}
		}

		/// <summary>
		/// Finds provider
		/// </summary>
		/// <param name="providerName"></param>
		/// <returns></returns>
		internal static ProviderInfo FindProvider(List<ProviderInfo> providers, string providerName)
		{
			foreach (ProviderInfo provider in providers)
			{
				if (provider.Name == providerName)
				{
					return provider;
				}
			}
			return null;
		}

		/// <summary>
		/// Changes provider enabled status
		/// </summary>
		internal static void SetProviderEnableStatus(string providerName, bool enabled)
		{
			foreach (ProviderInfo provider in _installedProviders)
			{
				if (provider.Name == providerName)
				{
					// Disabled property
					provider.Disabled = !enabled;

					if (enabled)
					{
						// enable the provider
						bool hasConflict;
						if (ApplyLoadedProvider(provider, out hasConflict))
						{
							provider.Loaded = true;
						}
					}
					else
					{
						provider.Loaded = false;

						// if user requested to disable the plugin
						Monitor.Enter(_loadedProviders);
						try
						{
							foreach (KeyValuePair<ProviderType, string> providerType in provider.Providers)
							{
								// remove the loaded key
								_loadedProviders.Remove(providerType.Key.ToString());
							}
						}
						finally
						{
							Monitor.Exit(_loadedProviders);
						}
					}
				}
			}
		}

		/// <summary>
		/// Reads then loads installed third-party provider extensions
		/// </summary>
		private static void ReadInstalledProviders()
		{
			string[] providersList = Directory.GetFiles(ProvidersLocation,
									 Consts.FilesConsts.File_ProviderInfoExt,
									 SearchOption.TopDirectoryOnly);

			if (_installedProviders == null)
				_installedProviders = new List<ProviderInfo>();
			else
				_installedProviders.Clear();

			foreach (string providerConfig in providersList)
			{
				// reads provider info from xml file
				ProviderInfo info = ReadProviderInfo(providerConfig);
				_installedProviders.Add(info);
			}

			LoadInstalledProviders(_installedProviders);
		}

		private static void LoadInstalledProviders(List<ProviderInfo> providersList)
		{
			try
			{
				ProviderInfo info;
				for (int i = 0; i < providersList.Count; i++)
				{
					info = providersList[i];

					// if provider is disabled don't do anything
					if (info.Disabled)
						continue;

					bool hasConflict;
					if (ApplyLoadedProvider(info, out hasConflict))
					{
						info.Loaded = true;
					}

					//providersList[i] = info; // required for structs
				}
			}
			catch (Exception ex)
			{
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, "Error in loading providers!", string.Empty);
			}
		}

		/// <summary>
		/// Applies the loaded provider and makes it ready to use
		/// </summary>
		/// <param name="hasConflict">If this provider has any conflicts</param>
		/// <returns>True ,if it was successfull to load the provider</returns>
		private static bool ApplyLoadedProvider(ProviderInfo info, out bool hasConflict)
		{
			hasConflict = false;
			bool loaded = false;

			foreach (KeyValuePair<ProviderType, string> providerType in info.Providers)
			{

				Type classType;
				try
				{
					// load the provider type
					classType = Type.GetType(providerType.Value);

					if (classType != null)
					{
						// at least one provider is loaded
						loaded = true;

						// if there is already key for this provider type
						if (_loadedProviders.ContainsKey(providerType.Key.ToString()))
							hasConflict = true;

						// Set to loaded results
						SetProviderType(providerType.Key, classType);
					}
				}
				catch (Exception ex)
				{
					if (Systems.LogSystem.ErrorLogEnabled)
						Systems.LogSystem.LogError(ex, "Failed to load provider: Name=" + info.Name
							+ " ProviderType=" + providerType.Key.ToString()
							+ " TypeName=" + providerType.Value, string.Empty);

					// continue to next provider
				}
			}
			return loaded;
		}

		/// <summary>
		/// Reads provider info config from specifed provider xml file
		/// </summary>
		private static ProviderInfo ReadProviderInfo(string providerXmlFile)
		{
			ProviderInfo result;
			try
			{
				XmlDocument xml = new XmlDocument();

				// load the provider file
				xml.Load(providerXmlFile);
				XmlNode rootNode = xml.SelectSingleNode("provider");

				// read data from xml data file
				result = ProviderInfo.ReadFromXml(rootNode);

				// disabled state
				result.Disabled = Configurations.Providers.IsProviderDisabled(result.Name);

				return result;
			}
			catch (Exception ex)
			{
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, "Failed to load a provider config: xmlFile= " + Path.GetFileName(providerXmlFile), string.Empty);
			}
			return new ProviderInfo();
		}


		/// <summary>
		/// Gets specified provider class type.
		/// </summary>
		private static Type GetProviderType(ProviderType providerType)
		{
			return (Type)_loadedProviders[providerType.ToString()];
		}

		/// <summary>
		/// Sets specified provider class type
		/// </summary>
		private static void SetProviderType(ProviderType providerType, Type classType)
		{
			_loadedProviders[providerType.ToString()] = classType;
		}

		/// <summary>
		/// Load providers list from default configuration file, obsoleted.
		/// </summary>
		[Obsolete("Builtin providers will be called instead.")]
		private static void LoadDefaultProviders()
		{
			try
			{
				XmlDocument xml = new XmlDocument();

				// load the assembly file
				xml.Load(ProvidersConfigFile);

				XmlNode rootNode = xml.SelectSingleNode("ASPorxyProviders");
				string activeProvider = rootNode.Attributes["active"].Value;
				XmlNode activeNode = null;

				foreach (XmlNode node in rootNode.ChildNodes)
				{
					// looking for specified provider
					if (node.Attributes["name"].Value == activeProvider)
					{
						activeNode = node;
						break;
					}
				}

				// Wrong name specified in configuration
				if (activeNode == null)
					return;

				foreach (XmlNode node in activeNode.ChildNodes)
				{
					if (node.NodeType == XmlNodeType.Comment)
						continue;

					string name = node.Attributes["name"].Value;
					string typeName = node.Attributes["typeName"].Value;

					try
					{
						// load specified class name
						Type classType = Type.GetType(typeName);

						// add to the collection
						if (classType != null)
							_loadedProviders[name] = classType;
					}
					catch (Exception ex)
					{
						// No error
						// The default provider will be used
						if (Systems.LogSystem.ErrorLogEnabled)
							Systems.LogSystem.LogError(ex, "Failed to load a provider: Name=" + name + " TypeName=" + typeName, string.Empty);
					}
				}
			}
			catch (Exception ex)
			{
				if (Systems.LogSystem.ErrorLogEnabled)
					Systems.LogSystem.LogError(ex, "Error in providers loading operation!", string.Empty);

				// No error
				// The default providers will be used
			}
		}

		private static object InvokeDefaultCreateInstance(Type type)
		{
			// Get the default constructor
			ConstructorInfo constructor = type.GetConstructor(new Type[] { });
			if (constructor != null)
			{
				return constructor.Invoke(new object[] { });
			}
			return false;
		}

		private static string ProvidersConfigFile
		{
			get
			{
				string config = ConfigurationManager.AppSettings["DefaultProvidersXml"];
				if (HttpContext.Current != null)
					config = HttpContext.Current.Server.MapPath(config);

				return config;
			}
		}
		#endregion

	}
}