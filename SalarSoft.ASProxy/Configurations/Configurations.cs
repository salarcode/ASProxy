using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web;
using System.Xml;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;

namespace SalarSoft.ASProxy
{
	///<summary>
	/// ASProxy Configurations
	///</summary>
	public static class Configurations
	{
		#region variables
		private static Hashtable _customConfig;
		private static AdminUIConfig _adminUI;
		private static ProvidersConfig _providers;
		private static PagesConfig _pages;
		private static WebDataConfig _webData;
		private static AuthenticationConfig _authentication;
		private static ImageCompressorConfig _imageCompressor;
		private static AutoUpdateConfig _autoUpdate;
		private static LogSystemConfig _logSystem;
		private static NetProxyConfig _netProxy;
		private static UserOptionsConfig _userOptions;
		private static UserAccessControlConfig _userAccessControlConfig;
		#endregion

		#region properties
		public static AdminUIConfig AdminUI
		{
			get { return Configurations._adminUI; }
			set { Configurations._adminUI = value; }
		}
		public static Hashtable CustomConfig
		{
			get
			{
				if (_customConfig == null)
					_customConfig = new Hashtable();
				return _customConfig;
			}
			set { _customConfig = value; }
		}
		public static ProvidersConfig Providers
		{
			get { return Configurations._providers; }
			set { Configurations._providers = value; }
		}
		public static WebDataConfig WebData
		{
			get { return Configurations._webData; }
			set { Configurations._webData = value; }
		}
		public static PagesConfig Pages
		{
			get { return Configurations._pages; }
			set { Configurations._pages = value; }
		}
		public static AuthenticationConfig Authentication
		{
			get { return Configurations._authentication; }
			set { Configurations._authentication = value; }
		}
		public static ImageCompressorConfig ImageCompressor
		{
			get { return Configurations._imageCompressor; }
			set { Configurations._imageCompressor = value; }
		}
		public static AutoUpdateConfig AutoUpdate
		{
			get { return Configurations._autoUpdate; }
			set { Configurations._autoUpdate = value; }
		}
		public static LogSystemConfig LogSystem
		{
			get { return Configurations._logSystem; }
			set { Configurations._logSystem = value; }
		}
		public static NetProxyConfig NetProxy
		{
			get { return Configurations._netProxy; }
			set { Configurations._netProxy = value; }
		}
		public static UserOptionsConfig UserOptions
		{
			get { return Configurations._userOptions; }
			set { Configurations._userOptions = value; }
		}
		public static UserAccessControlConfig UserAccessControl
		{
			get { return Configurations._userAccessControlConfig; }
			set { Configurations._userAccessControlConfig = value; }
		}
		#endregion

		#region public methods
		static Configurations()
		{
			_adminUI = new AdminUIConfig();
			_providers = new ProvidersConfig();
			_pages = new PagesConfig();
			_webData = new WebDataConfig();
			_authentication = new AuthenticationConfig();
			_imageCompressor = new ImageCompressorConfig();
			_autoUpdate = new AutoUpdateConfig();
			_logSystem = new LogSystemConfig();
			_netProxy = new NetProxyConfig();
			_userOptions = new UserOptionsConfig();
			_userAccessControlConfig = new UserAccessControlConfig();
			ReadSettings();
		}
		public static void SaveSettings()
		{
			try
			{
				XmlNode activeNode;
				XmlNode rootNode;
				XmlNode node;
				XmlAttribute attribute;
				XmlDocument xmlDoc = new XmlDocument();

				xmlDoc.LoadXml("<?xml version='1.0' encoding='utf-8' ?><Configurations></Configurations>");
				rootNode = xmlDoc.SelectSingleNode("Configurations");

				// administration UI-------
				_adminUI.SaveToXml(xmlDoc, rootNode);

				// providers -------
				_providers.SaveToXml(xmlDoc, rootNode);

				// webData -------
				_webData.SaveToXml(xmlDoc, rootNode);

				// pages -------
				_pages.SaveToXml(xmlDoc, rootNode);

				// authentication ------
				_authentication.SaveToXml(xmlDoc, rootNode);

				// imageCompressor ------
				_imageCompressor.SaveToXml(xmlDoc, rootNode);

				// autoUpdate -------
				_autoUpdate.SaveToXml(xmlDoc, rootNode);

				// logSystem -------
				_logSystem.SaveToXml(xmlDoc, rootNode);

				// netProxy -------
				_netProxy.SaveToXml(xmlDoc, rootNode);

				// Users config
				_userOptions.SaveToXml(xmlDoc, rootNode);

				// UAC config
				_userAccessControlConfig.SaveToXml(xmlDoc, rootNode);

				// customConfig ---------
				activeNode = xmlDoc.CreateElement("customConfig");
				rootNode.AppendChild(activeNode);

				if (_customConfig != null && _customConfig.Count > 0)
				{
					foreach (DictionaryEntry entry in _customConfig)
					{
						node = xmlDoc.CreateAttribute("config");
						activeNode.AppendChild(node);

						attribute = xmlDoc.CreateAttribute("name");
						attribute.Value = entry.Key.ToString();
						node.Attributes.Append(attribute);

						attribute = xmlDoc.CreateAttribute("value");
						attribute.Value = entry.Value.ToString();
						node.Attributes.Append(attribute);
					}
				}


				xmlDoc.Save(ConfigurationsFile);
			}
			catch
			{
			}
		}
		public static void ReloadSettings()
		{
			ReadSettings();
		}
		#endregion

		#region private methods
		private static string ConfigurationsFile
		{
			get
			{
				string config = ConfigurationManager.AppSettings["ConfigurationsXml"];
				if (HttpContext.Current != null)
					config = HttpContext.Current.Server.MapPath(config);
				return config;
			}
		}
		private static void ReadSettings()
		{
			try
			{
				XmlNode activeNode;
				XmlNode rootNode;
				XmlDocument xmlDoc = new XmlDocument();

				// load the assembly file
				xmlDoc.Load(ConfigurationsFile);

				// The root node
				rootNode = xmlDoc.SelectSingleNode("Configurations");


				// administration UI
				_adminUI.ReadFromXml(rootNode);

				// providers -------
				_providers.ReadFromXml(rootNode);

				// webData -------
				_webData.ReadFromXml(rootNode);

				// pages -------
				_pages.ReadFromXml(rootNode);

				// authentication ------
				_authentication.ReadFromXml(rootNode);

				// imageCompressor ------
				_imageCompressor.ReadFromXml(rootNode);

				// autoUpdate -------
				_autoUpdate.ReadFromXml(rootNode);

				// logSystem -------
				_logSystem.ReadFromXml(rootNode);

				// netProxy -------
				_netProxy.ReadFromXml(rootNode);

				// UserOptions--------
				_userOptions.ReadFromXml(rootNode);

				// UAC--------
				_userAccessControlConfig.ReadFromXml(rootNode);

				// Custom config -------
				activeNode = rootNode.SelectSingleNode("customConfig");
				if (activeNode.ChildNodes.Count > 0)
					_customConfig = new Hashtable();
				foreach (XmlNode childNode in activeNode.ChildNodes)
				{
					if (childNode.NodeType != XmlNodeType.Comment)
					{
						_customConfig.Add(childNode.Attributes["name"].Value,
							childNode.Attributes["value"].Value);
					}
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Configuration can not load or parse the xml config file.", ex);
			}
		}
		#endregion

		#region config classes
		private interface IConfigSection
		{
			void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode);
			void ReadFromXml(XmlNode rootNode);
		}
		public class ProvidersConfig : IConfigSection
		{
			public bool ProviderExtensionsEnabled;
			public bool PluginsEnabled;
			public StringCollection DisabledPlugins;
			public StringCollection DisabledProviders;

			public bool IsPluginDisabled(string pluginName)
			{
				return DisabledPlugins.Contains(pluginName);
			}
			public bool IsProviderDisabled(string providerName)
			{
				return DisabledProviders.Contains(providerName);
			}
			public void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode)
			{
				XmlNode node;
				XmlAttribute attribute;

				XmlNode activeNode = xmlDoc.CreateElement("providers");
				rootNode.AppendChild(activeNode);

				// engine node
				node = xmlDoc.CreateElement("providerExtensions");
				activeNode.AppendChild(node);
				attribute = xmlDoc.CreateAttribute("enabled");
				attribute.Value = this.ProviderExtensionsEnabled.ToString();
				node.Attributes.Append(attribute);

				// disabled plugins node
				XmlNode disabledProviderNode = xmlDoc.CreateElement("disabledProviders");
				node.AppendChild(disabledProviderNode);
				for (int i = 0; i < DisabledProviders.Count; i++)
				{
					XmlNode nameNode = xmlDoc.CreateElement("provider");
					disabledProviderNode.AppendChild(nameNode);

					attribute = xmlDoc.CreateAttribute("name");
					attribute.Value = DisabledProviders[i];
					nameNode.Attributes.Append(attribute);
				}

				// plugins node
				node = xmlDoc.CreateElement("plugins");
				activeNode.AppendChild(node);
				attribute = xmlDoc.CreateAttribute("enabled");
				attribute.Value = this.PluginsEnabled.ToString();
				node.Attributes.Append(attribute);

				// disabled plugins node
				XmlNode disabledNode = xmlDoc.CreateElement("disabledPlugins");
				node.AppendChild(disabledNode);
				for (int i = 0; i < DisabledPlugins.Count; i++)
				{
					XmlNode nameNode = xmlDoc.CreateElement("plugin");
					disabledNode.AppendChild(nameNode);

					attribute = xmlDoc.CreateAttribute("name");
					attribute.Value = DisabledPlugins[i];
					nameNode.Attributes.Append(attribute);
				}
			}
			public void ReadFromXml(XmlNode rootNode)
			{
				XmlNode node = rootNode.SelectSingleNode("providers/providerExtensions");
				this.ProviderExtensionsEnabled = Convert.ToBoolean(node.Attributes["enabled"].Value);

				// disabled providers list
				node = rootNode.SelectSingleNode("providers/providerExtensions/disabledProviders");
				this.DisabledProviders = new StringCollection();
				foreach (XmlNode childNode in node.ChildNodes)
				{
					DisabledProviders.Add(childNode.Attributes["name"].Value);
				}

				// plugins
				node = rootNode.SelectSingleNode("providers/plugins");
				this.PluginsEnabled = Convert.ToBoolean(node.Attributes["enabled"].Value);

				// disabled plugins list
				node = rootNode.SelectSingleNode("providers/plugins/disabledPlugins");
				this.DisabledPlugins = new StringCollection();
				foreach (XmlNode childNode in node.ChildNodes)
				{
					DisabledPlugins.Add(childNode.Attributes["name"].Value);
				}
			}
		}
		public class PagesConfig : IConfigSection
		{
			public string UILanguage;

			public CultureInfo GetUiLanguage()
			{
				try
				{
					return new CultureInfo(UILanguage);
				}
				catch (Exception)
				{
					UILanguage = "en-us";
					return CultureInfo.CurrentUICulture;
				}
			}
			public void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode)
			{
				XmlAttribute attribute;

				XmlNode pagesNode = xmlDoc.CreateElement("pages");
				rootNode.AppendChild(pagesNode);


				attribute = xmlDoc.CreateAttribute("uiLanguage");
				attribute.Value = this.UILanguage;
				pagesNode.Attributes.Append(attribute);
			}
			public void ReadFromXml(XmlNode rootNode)
			{
				XmlNode node = rootNode.SelectSingleNode("pages");
				this.UILanguage = node.Attributes["uiLanguage"].Value;
			}
		}
		public class WebDataConfig : IConfigSection
		{
			/// <summary>
			/// In bytes, -1 for unlimited
			/// </summary>
			public long MaxContentLength;

			/// <summary>
			/// Length of time in milliseconds, before the request times out.
			/// </summary>
			public int RequestTimeout;

			/// <summary>
			/// Length of time in milliseconds when writing to or reading from a stream (Back-end site).
			/// </summary>
			public int RequestReadWriteTimeOut;

			/// <summary>
			/// Send ASProxy signature header to Back-end and Front-end users.
			/// </summary>
			public bool SendSignature;

			/// <summary>
			/// Preferred encoding for unrecognized page encodings.
			/// </summary>
			public string PreferredLocalEncoding;

			/// <summary>
			/// Back-end UserAgent behaviour.
			/// </summary>
			public UserAgentMode UserAgent;

			/// <summary>
			/// UserAgent for Back-End.
			/// </summary>
			public string UserAgentCustom;

			/// <summary>
			/// Downloader Resume-Support feature.
			/// </summary>
			public bool Downloader_ResumeSupport;

			/// <summary>
			/// Enables Resume-Support even if back-end site doesn't support it.
			/// Warning, ASProxy will use memory to buffer the whole download data.
			/// </summary>
			public bool Downloader_ForceResumeSupport;

			/// <summary>
			/// Downloader specific. In bytes, -1 for unlimited.
			/// </summary>
			public long Downloader_MaxContentLength;

			/// <summary>
			/// Downloader specific. Length of time in milliseconds, before the request times out.
			/// </summary>
			public int Downloader_Timeout;

			/// <summary>
			/// Downloader specific. Length of time in milliseconds when writing to or reading from a stream (Back-end site).
			/// </summary>
			public int Downloader_ReadWriteTimeOut;

			public void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode)
			{
				XmlAttribute attribute;

				XmlNode webDataNode = xmlDoc.CreateElement("webData");
				rootNode.AppendChild(webDataNode);


				attribute = xmlDoc.CreateAttribute("maxContentLength");
				attribute.Value = this.MaxContentLength.ToString();
				webDataNode.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("requestTimeout");
				attribute.Value = this.RequestTimeout.ToString();
				webDataNode.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("requestReadWriteTimeOut");
				attribute.Value = this.RequestReadWriteTimeOut.ToString();
				webDataNode.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("sendSignature");
				attribute.Value = this.SendSignature.ToString();
				webDataNode.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("preferredLocalEncoding");
				attribute.Value = this.PreferredLocalEncoding;
				webDataNode.Attributes.Append(attribute);

				// userAgent node
				XmlNode userAgentNode = xmlDoc.CreateElement("userAgent");
				webDataNode.AppendChild(userAgentNode);

				attribute = xmlDoc.CreateAttribute("mode");
				attribute.Value = this.UserAgent.ToString();
				userAgentNode.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("custom");
				attribute.Value = this.UserAgentCustom;
				userAgentNode.Attributes.Append(attribute);

				// downloader node
				XmlNode downloader = xmlDoc.CreateElement("downloader");
				webDataNode.AppendChild(downloader);

				attribute = xmlDoc.CreateAttribute("resumeSupport");
				attribute.Value = this.Downloader_ResumeSupport.ToString();
				downloader.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("forceResumeSupport");
				attribute.Value = this.Downloader_ForceResumeSupport.ToString();
				downloader.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("maxContentLength");
				attribute.Value = this.Downloader_MaxContentLength.ToString();
				downloader.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("requestTimeout");
				attribute.Value = this.Downloader_Timeout.ToString();
				downloader.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("requestReadWriteTimeOut");
				attribute.Value = this.Downloader_ReadWriteTimeOut.ToString();
				downloader.Attributes.Append(attribute);


			}
			public void ReadFromXml(XmlNode rootNode)
			{
				this.RequestTimeout = Consts.BackEndConenction.RequestTimeOut;
				this.RequestReadWriteTimeOut = Consts.BackEndConenction.RequestFormReadWriteTimeOut;

				XmlNode node = rootNode.SelectSingleNode("webData");
				this.MaxContentLength = Convert.ToInt64(node.Attributes["maxContentLength"].Value);
				this.RequestTimeout = Convert.ToInt32(node.Attributes["requestTimeout"].Value);
				this.RequestReadWriteTimeOut = Convert.ToInt32(node.Attributes["requestReadWriteTimeOut"].Value);
				this.SendSignature = Convert.ToBoolean(node.Attributes["sendSignature"].Value);
				this.PreferredLocalEncoding = node.Attributes["preferredLocalEncoding"].Value;

				// userAgent node
				XmlNode userAgentNode = node.SelectSingleNode("userAgent");
				this.UserAgent = (UserAgentMode)Enum.Parse(typeof(UserAgentMode), userAgentNode.Attributes["mode"].Value, true);
				this.UserAgentCustom = userAgentNode.Attributes["custom"].Value;

				// downloader node
				XmlNode downloaderNode = node.SelectSingleNode("downloader");
				this.Downloader_ResumeSupport = Convert.ToBoolean(downloaderNode.Attributes["resumeSupport"].Value);
				this.Downloader_ForceResumeSupport = Convert.ToBoolean(downloaderNode.Attributes["forceResumeSupport"].Value);
				this.Downloader_MaxContentLength = Convert.ToInt64(downloaderNode.Attributes["maxContentLength"].Value);
				this.Downloader_Timeout = Convert.ToInt32(downloaderNode.Attributes["requestTimeout"].Value);
				this.Downloader_ReadWriteTimeOut = Convert.ToInt32(downloaderNode.Attributes["requestReadWriteTimeOut"].Value);
			}

			public enum UserAgentMode { Default = 0, ASProxy = 1, Custom = 2 };
		}
		public class AuthenticationConfig : IConfigSection
		{
			public bool Enabled;
			public List<User> Users;


			public bool IsUserAuthenticated(string userName, string password)
			{
				if (Users != null)
				{
					userName = userName.ToLower();
					password = password.ToLower();
					foreach (User user in Users)
					{
						if (userName == user.UserName.ToLower() &&
							password == user.Password.ToLower())
							return true;
					}
				}
				return false;
			}
			public User GetByUsername(string userName)
			{
				if (Users != null)
				{
					userName = userName.ToLower();
					foreach (User user in Users)
					{
						if (userName == user.UserName.ToLower())
							return user;
					}
				}
				return new User();
			}
			public bool HasPermission(string userName, UserPermission permission)
			{
				if (Users != null)
				{
					userName = userName.ToLower();
					foreach (User user in Users)
					{
						if (userName == user.UserName.ToLower())
						{
							switch (permission)
							{
								case UserPermission.Pages:
									return user.Pages;

								case UserPermission.Images:
									return user.Images;

								case UserPermission.Downloads:
									return user.Downloads;

								default:
									return false;
							}
						}
					}
				}
				return false;
			}
			public void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode)
			{
				XmlNode node;
				XmlAttribute attribute;

				XmlNode activeNode = xmlDoc.CreateElement("authentication");
				rootNode.AppendChild(activeNode);

				attribute = xmlDoc.CreateAttribute("enabled");
				attribute.Value = this.Enabled.ToString();
				activeNode.Attributes.Append(attribute);

				if (this.Users != null && this.Users.Count > 0)
				{
					foreach (AuthenticationConfig.User user in this.Users)
					{
						node = xmlDoc.CreateElement("user");
						activeNode.AppendChild(node);

						attribute = xmlDoc.CreateAttribute("userName");
						attribute.Value = user.UserName;
						node.Attributes.Append(attribute);

						attribute = xmlDoc.CreateAttribute("password");
						attribute.Value = user.Password;
						node.Attributes.Append(attribute);

						attribute = xmlDoc.CreateAttribute("pages");
						attribute.Value = user.Pages.ToString();
						node.Attributes.Append(attribute);

						attribute = xmlDoc.CreateAttribute("images");
						attribute.Value = user.Images.ToString();
						node.Attributes.Append(attribute);

						attribute = xmlDoc.CreateAttribute("downloads");
						attribute.Value = user.Downloads.ToString();
						node.Attributes.Append(attribute);

					}
				}
			}
			public void ReadFromXml(XmlNode rootNode)
			{
				XmlNode node = rootNode.SelectSingleNode("authentication");

				this.Enabled = Convert.ToBoolean(node.Attributes["enabled"].Value);

				this.Users = new List<AuthenticationConfig.User>();
				foreach (XmlNode childNode in node.ChildNodes)
				{
					if (childNode.NodeType == XmlNodeType.Comment) continue;

					AuthenticationConfig.User user = new AuthenticationConfig.User();
					user.UserName = childNode.Attributes["userName"].Value;
					user.Password = childNode.Attributes["password"].Value;
					user.Pages = Convert.ToBoolean(childNode.Attributes["pages"].Value);
					user.Images = Convert.ToBoolean(childNode.Attributes["images"].Value);
					user.Downloads = Convert.ToBoolean(childNode.Attributes["downloads"].Value);
					this.Users.Add(user);
				}
			}

			public enum UserPermission { Pages, Images, Downloads };
			public struct User
			{
				public string UserName;
				public string Password;
				public bool Pages;
				public bool Images;
				public bool Downloads;
			}
		}
		public class ImageCompressorConfig : IConfigSection
		{
			public bool Enabled;
			public int Quality;

			public void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode)
			{
				XmlAttribute attribute;

				XmlNode activeNode = xmlDoc.CreateElement("imageCompressor");
				rootNode.AppendChild(activeNode);

				attribute = xmlDoc.CreateAttribute("enabled");
				attribute.Value = this.Enabled.ToString();
				activeNode.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("quality");
				attribute.Value = this.Quality.ToString();
				activeNode.Attributes.Append(attribute);
			}
			public void ReadFromXml(XmlNode rootNode)
			{
				XmlNode node = rootNode.SelectSingleNode("imageCompressor"); ;

				this.Enabled = Convert.ToBoolean(node.Attributes["enabled"].Value);
				this.Quality = Convert.ToInt32(node.Attributes["quality"].Value);
			}
		}
		public class AutoUpdateConfig : IConfigSection
		{
			public string UpdateInfoUrl;
			public bool Engine;
			public bool Plugins;
			public bool Providers;

			public void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode)
			{
				XmlNode node;
				XmlAttribute attribute;

				node = xmlDoc.CreateElement("autoUpdate");
				rootNode.AppendChild(node);

				attribute = xmlDoc.CreateAttribute("updateInfoUrl");
				attribute.Value = this.UpdateInfoUrl;
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("engine");
				attribute.Value = this.Engine.ToString();
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("plugins");
				attribute.Value = this.Plugins.ToString();
				node.Attributes.Append(attribute);

			}
			public void ReadFromXml(XmlNode rootNode)
			{
				XmlNode node = rootNode.SelectSingleNode("autoUpdate");
				this.UpdateInfoUrl = node.Attributes["updateInfoUrl"].Value;
				this.Engine = Convert.ToBoolean(node.Attributes["engine"].Value);
				this.Plugins = Convert.ToBoolean(node.Attributes["plugins"].Value);
			}
		}
		public class LogSystemConfig : IConfigSection
		{
			public long MaxFileSize;
			public string FileFormat;
			public bool ActivityLog_Enabled;
			public string ActivityLog_Location;
			public bool ActivityLog_Pages;
			public bool ActivityLog_Images;

			public bool ErrorLog_Enabled;
			public string ErrorLog_location;

			public void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode)
			{
				XmlNode node;
				XmlAttribute attribute;

				XmlNode activeNode = xmlDoc.CreateElement("logSystem");
				rootNode.AppendChild(activeNode);

				attribute = xmlDoc.CreateAttribute("fileFormat");
				attribute.Value = this.FileFormat;
				activeNode.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("maxFileSize");
				attribute.Value = this.MaxFileSize.ToString();
				activeNode.Attributes.Append(attribute);

				// logSystem/activityLog
				node = xmlDoc.CreateElement("activityLog");
				activeNode.AppendChild(node);

				attribute = xmlDoc.CreateAttribute("enabled");
				attribute.Value = this.ActivityLog_Enabled.ToString();
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("location");
				attribute.Value = this.ActivityLog_Location;
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("pages");
				attribute.Value = this.ActivityLog_Pages.ToString();
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("images");
				attribute.Value = this.ActivityLog_Images.ToString();
				node.Attributes.Append(attribute);

				// logSystem/errorLog
				node = xmlDoc.CreateElement("errorLog");
				activeNode.AppendChild(node);

				attribute = xmlDoc.CreateAttribute("enabled");
				attribute.Value = this.ErrorLog_Enabled.ToString();
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("location");
				attribute.Value = this.ErrorLog_location;
				node.Attributes.Append(attribute);
			}
			public void ReadFromXml(XmlNode rootNode)
			{
				XmlNode node = rootNode.SelectSingleNode("logSystem");
				this.FileFormat = node.Attributes["fileFormat"].Value;
				this.MaxFileSize = Convert.ToInt64(node.Attributes["maxFileSize"].Value);

				node = rootNode.SelectSingleNode("logSystem/activityLog");
				this.ActivityLog_Enabled = Convert.ToBoolean(node.Attributes["enabled"].Value);
				this.ActivityLog_Location = node.Attributes["location"].Value;
				this.ActivityLog_Pages = Convert.ToBoolean(node.Attributes["pages"].Value);
				this.ActivityLog_Images = Convert.ToBoolean(node.Attributes["images"].Value);

				node = rootNode.SelectSingleNode("logSystem/errorLog");
				this.ErrorLog_Enabled = Convert.ToBoolean(node.Attributes["enabled"].Value);
				this.ErrorLog_location = node.Attributes["location"].Value;
			}
		}
		public class NetProxyConfig : IConfigSection
		{
			private IWebProxy _webProxy;

			/// <summary>
			/// Determines proxy configuration to connect to the back-end websites.
			/// </summary>
			public NetProxyMode Mode;

			/// <summary>
			/// Proxy address for custom configuration.
			/// </summary>
			public string Address;

			/// <summary>
			/// Proxy port for custom configuration.
			/// </summary>
			public int Port;

			/// <summary>
			/// Is authentication required for specified custom proxy.
			/// </summary>
			public bool Authentication;

			/// <summary>
			/// Custom proxy username.
			/// </summary>
			public string Authentication_Username;

			/// <summary>
			/// Custom proxy password.
			/// </summary>
			public string Authentication_Password;

			public void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode)
			{
				XmlAttribute attribute;
				XmlNode activeNode;

				XmlNode node = xmlDoc.CreateElement("netProxy");
				rootNode.AppendChild(node);
				activeNode = node;

				attribute = xmlDoc.CreateAttribute("mode");
				attribute.Value = this.Mode.ToString();
				node.Attributes.Append(attribute);

				// netProxy/custom
				node = xmlDoc.CreateElement("custom");
				activeNode.AppendChild(node);
				activeNode = node;

				attribute = xmlDoc.CreateAttribute("address");
				attribute.Value = this.Address;
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("port");
				attribute.Value = this.Port.ToString();
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("authentication");
				attribute.Value = this.Authentication.ToString();
				node.Attributes.Append(attribute);

				// netProxy/custom/authentication
				node = xmlDoc.CreateElement("authentication");
				activeNode.AppendChild(node);
				activeNode = node;

				attribute = xmlDoc.CreateAttribute("userName");
				attribute.Value = this.Authentication_Username;
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("password");
				attribute.Value = this.Authentication_Password;
				node.Attributes.Append(attribute);
			}
			public void ReadFromXml(XmlNode rootNode)
			{
				XmlNode node = rootNode.SelectSingleNode("netProxy");
				this.Mode = (NetProxyConfig.NetProxyMode)Enum.Parse(typeof(NetProxyConfig.NetProxyMode),
									node.Attributes["mode"].Value, true);
				node = rootNode.SelectSingleNode("netProxy/custom");
				this.Address = node.Attributes["address"].Value;
				this.Port = Convert.ToInt32(node.Attributes["port"].Value);
				this.Authentication = Convert.ToBoolean(node.Attributes["authentication"].Value);

				node = rootNode.SelectSingleNode("netProxy/custom/authentication");
				this.Authentication_Username = node.Attributes["userName"].Value;
				this.Authentication_Password = node.Attributes["password"].Value;
			}
			public bool WebProxyEnabled
			{
				get
				{
					return (Mode == NetProxyMode.Custom || Mode == NetProxyMode.SystemDefault);
				}
			}
			public IWebProxy GenerateWebProxy()
			{
				if (_webProxy == null)
				{
					switch (Mode)
					{
						case NetProxyMode.Direct:
							_webProxy = new WebProxy();
							break;

						case NetProxyMode.SystemDefault:
							_webProxy = HttpWebRequest.DefaultWebProxy;
							break;

						case NetProxyMode.Custom:
							_webProxy = new WebProxy(Address, Port);
							if (Authentication)
							{
								_webProxy.Credentials =
									new NetworkCredential(Authentication_Username, Authentication_Password);
							}
							break;
					}
				}
				return _webProxy;
			}

			public enum NetProxyMode { Direct = 0, SystemDefault = 1, Custom = 2 }
		}
		public class UserOptionsConfig : IConfigSection
		{
			public struct UserConfig
			{
				public bool Enabled;
				public bool Changeable;
			}

			public UserConfig Cookies;
			public UserConfig TempCookies;
			public UserConfig Images;
			public UserConfig Links;
			public UserConfig Frames;
			public UserConfig SubmitForms;
			public UserConfig RemoveObjects;
			public UserConfig HttpCompression;
			public UserConfig EncodeUrl;
			public UserConfig OrginalUrl;
			public UserConfig PageTitle;
			public UserConfig ForceEncoding;
			public UserConfig RemoveScripts;
			public UserConfig RemoveImages;
			public UserConfig DocType;
			public UserConfig ImageCompressor;


			public void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode)
			{
				XmlNode activeNode;

				XmlNode node = xmlDoc.CreateElement("userOptions");
				rootNode.AppendChild(node);
				activeNode = node;

				// userOptions/options
				node = xmlDoc.CreateElement("options");
				activeNode.AppendChild(node);
				activeNode = node;

				// Save configurations
				SaveConfig(xmlDoc, activeNode, Cookies, "Cookies");
				SaveConfig(xmlDoc, activeNode, TempCookies, "TempCookies");
				SaveConfig(xmlDoc, activeNode, Images, "Images");
				SaveConfig(xmlDoc, activeNode, Links, "Links");
				SaveConfig(xmlDoc, activeNode, Frames, "Frames");
				SaveConfig(xmlDoc, activeNode, SubmitForms, "SubmitForms");
				SaveConfig(xmlDoc, activeNode, RemoveObjects, "RemoveObjects");
				SaveConfig(xmlDoc, activeNode, HttpCompression, "HttpCompression");
				SaveConfig(xmlDoc, activeNode, ImageCompressor, "ImageCompressor");
				SaveConfig(xmlDoc, activeNode, EncodeUrl, "EncodeUrl");
				SaveConfig(xmlDoc, activeNode, OrginalUrl, "OrginalUrl");
				SaveConfig(xmlDoc, activeNode, PageTitle, "PageTitle");
				SaveConfig(xmlDoc, activeNode, ForceEncoding, "ForceEncoding");
				SaveConfig(xmlDoc, activeNode, RemoveScripts, "RemoveScripts");
				SaveConfig(xmlDoc, activeNode, RemoveImages, "RemoveImages");
				SaveConfig(xmlDoc, activeNode, DocType, "DocType");
			}
			public void ReadFromXml(XmlNode rootNode)
			{
				XmlNode node = rootNode.SelectSingleNode("userOptions/options");

				Type optionType = typeof(UserOptionsConfig);
				foreach (XmlNode configNode in node.ChildNodes)
				{
					FieldInfo info = optionType.GetField(configNode.Name);
					if (info != null)
					{
						UserConfig readedConfig;
						readedConfig.Enabled = Convert.ToBoolean(configNode.InnerText);
						readedConfig.Changeable = Convert.ToBoolean(configNode.Attributes["changeable"].Value);

						info.SetValue(this, readedConfig);
					}
				}
			}
			void SaveConfig(XmlDocument xmlDoc, XmlNode baseNode, UserConfig config, string configName)
			{
				XmlNode configNode = xmlDoc.CreateElement(configName);
				baseNode.AppendChild(configNode);
				configNode.InnerText = config.Enabled.ToString();

				XmlAttribute attribute = xmlDoc.CreateAttribute("changeable");
				attribute.Value = config.Changeable.ToString();
				configNode.Attributes.Append(attribute);
			}
		}
		public class AdminUIConfig : IConfigSection
		{
			public bool IsAdminStarted;
			public string UserName;
			public string Password;
			public void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode)
			{

				XmlAttribute attribute;
				XmlNode activeNode;

				XmlNode node = xmlDoc.CreateElement("adminUI");
				rootNode.AppendChild(node);
				activeNode = node;

				attribute = xmlDoc.CreateAttribute("isAdminStarted");
				attribute.Value = this.IsAdminStarted.ToString();
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("userName");
				attribute.Value = this.UserName.ToString();
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("password");
				attribute.Value = this.Password.ToString();
				node.Attributes.Append(attribute);
			}
			public void ReadFromXml(XmlNode rootNode)
			{
				XmlNode node = rootNode.SelectSingleNode("adminUI");
				this.IsAdminStarted = Convert.ToBoolean(node.Attributes["isAdminStarted"].Value);
				this.UserName = node.Attributes["userName"].Value;
				this.Password = node.Attributes["password"].Value;
			}
		}
		public class UserAccessControlConfig : IConfigSection
		{
			private const string _IPRange = "range";
			private const string _IPSingle = "ip";

			public bool Enabled;
			public List<IPRange> BlockedRange;
			public List<IPRange> AllowedRange;
			public List<string> BlockedList;
			public List<string> AllowedList;

			public void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode)
			{
				XmlNode userAccessControl = xmlDoc.CreateElement("userAccessControl");
				rootNode.AppendChild(userAccessControl);

				// Enabled
				XmlAttribute attrEnabled = xmlDoc.CreateAttribute("enabled");
				attrEnabled.Value = Enabled.ToString();
				userAccessControl.Attributes.Append(attrEnabled);


				XmlNode blocked = xmlDoc.CreateElement("blocked");
				userAccessControl.AppendChild(blocked);

				XmlNode allowed = xmlDoc.CreateElement("allowed");
				userAccessControl.AppendChild(allowed);

				// White list
				if (AllowedRange != null)
					foreach (IPRange range in AllowedRange)
					{
						XmlNode xmlRange = xmlDoc.CreateElement(_IPRange);
						allowed.AppendChild(xmlRange);

						XmlAttribute xmlHigh = xmlDoc.CreateAttribute("high");
						XmlAttribute xmlLow = xmlDoc.CreateAttribute("low");

						xmlHigh.Value = range.High;
						xmlLow.Value = range.Low;

						xmlRange.Attributes.Append(xmlHigh);
						xmlRange.Attributes.Append(xmlLow);
					}
				if (AllowedList != null)
					foreach (string ip in AllowedList)
					{
						XmlNode xmlIP = xmlDoc.CreateElement(_IPSingle);
						allowed.AppendChild(xmlIP);

						XmlAttribute xmlSingle = xmlDoc.CreateAttribute("value");
						xmlSingle.Value = ip;

						xmlIP.Attributes.Append(xmlSingle);
					}

				// The black list
				if (BlockedRange != null)
					foreach (IPRange range in BlockedRange)
					{
						XmlNode xmlRange = xmlDoc.CreateElement(_IPRange);
						blocked.AppendChild(xmlRange);

						XmlAttribute xmlHigh = xmlDoc.CreateAttribute("high");
						XmlAttribute xmlLow = xmlDoc.CreateAttribute("low");

						xmlHigh.Value = range.High;
						xmlLow.Value = range.Low;

						xmlRange.Attributes.Append(xmlHigh);
						xmlRange.Attributes.Append(xmlLow);
					}
				if (BlockedList != null)
					foreach (string ip in BlockedList)
					{
						XmlNode xmlIP = xmlDoc.CreateElement(_IPSingle);
						blocked.AppendChild(xmlIP);

						XmlAttribute xmlSingle = xmlDoc.CreateAttribute("value");
						xmlSingle.Value = ip;

						xmlIP.Attributes.Append(xmlSingle);
					}
			}

			public void ReadFromXml(XmlNode rootNode)
			{
				XmlNode userAccessControl = rootNode.SelectSingleNode("userAccessControl");
				this.Enabled = Convert.ToBoolean(userAccessControl.Attributes["enabled"].Value);

				XmlNode blocked = userAccessControl.SelectSingleNode("blocked");
				XmlNode allowed = userAccessControl.SelectSingleNode("allowed");

				// White list
				if (allowed != null)
					foreach (XmlNode node in allowed)
					{
						if (node.NodeType == XmlNodeType.Comment)
							continue;

						if (node.Name == _IPRange)
						{
							IPRange range = new IPRange();
							range.High = node.Attributes["high"].Value;
							range.Low = node.Attributes["low"].Value;

							if (AllowedRange == null)
								AllowedRange = new List<IPRange>();

							// Add to allowed list
							AllowedRange.Add(range);
						}
						else if (node.Name == _IPSingle)
						{
							if (AllowedList == null)
								AllowedList = new List<string>();

							// Read and add it
							AllowedList.Add(node.Attributes["value"].Value);
						}
					}

				// The black list
				if (blocked != null && (AllowedRange == null && AllowedList == null))
					foreach (XmlNode node in blocked)
					{
						if (node.NodeType == XmlNodeType.Comment)
							continue;

						if (node.Name == _IPRange)
						{
							IPRange range = new IPRange();
							range.High = node.Attributes["high"].Value;
							range.Low = node.Attributes["low"].Value;

							if (BlockedRange == null)
								BlockedRange = new List<IPRange>();

							// Add to allowed list
							BlockedRange.Add(range);
						}
						else if (node.Name == _IPSingle)
						{
							if (BlockedList == null)
								BlockedList = new List<string>();

							// Read and add it
							BlockedList.Add(node.Attributes["value"].Value);
						}
					}
			}

			public struct IPRange
			{
				public string High;
				public string Low;
			}
		}
		#endregion
	}
}