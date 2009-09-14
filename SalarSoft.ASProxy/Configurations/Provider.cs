using System;
using System.Collections;
using System.Reflection;
using System.Xml;
using SalarSoft.ASProxy.BuiltIn;
using System.Web;
using System.Configuration;

namespace SalarSoft.ASProxy
{

	/// <summary>
	/// Extensions Provider Loader
	/// </summary>
	public static class Provider
	{
		#region variables
		private static Hashtable _implementation;
		#endregion

		#region properties
		/// <summary>
		/// Implementation list
		/// </summary>
		internal static Hashtable Implementation
		{
			get { return _implementation; }
			set { _implementation = value; }
		}
		#endregion

		#region public methods
		static Provider()
		{
			_implementation = new Hashtable();

			if (Configurations.Providers.EngineCanBeOverwritten)
				// the default providers will be used
				LoadProviders();
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
			}
			return null;
		}
		#endregion

		#region private methods
		/// <summary>
		/// Gets specified provider class type.
		/// </summary>
		private static Type GetProviderType(ProviderType providerType)
		{
			return (Type)_implementation[providerType.ToString()];
		}

		/// <summary>
		/// Load providers list from xml file
		/// </summary>
		private static void LoadProviders()
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
							_implementation[name] = classType;
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
				string config = ConfigurationManager.AppSettings["EngineProvidersXml"];
				if (HttpContext.Current != null)
					config = HttpContext.Current.Server.MapPath(config);

				return config;
			}
		}
		#endregion

	}
}