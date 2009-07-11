using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web;
using System.Xml;

namespace SalarSoft.ASProxy
{
	///<summary>
	/// ASProxy Configurations
	///</summary>
	public static class Configurations
	{
		#region variables
		private static Hashtable _customConfig;
		private static ProvidersConfig _providers;
		private static WebDataConfig _webData;
		private static AuthenticationConfig _authentication;
		private static ImageCompressorConfig _imageCompressor;
		private static AutoUpdateConfig _autoUpdate;
		private static LogSystemConfig _logSystem;
		private static NetProxyConfig _netProxy;
		private static UserOptions _userOptions;
		#endregion

		#region properties
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
		public static UserOptions UserOptions
		{
			get { return Configurations._userOptions; }
			set { Configurations._userOptions = value; }
		}
		#endregion

		#region public methods
		static Configurations()
		{
			ReadSettings();
		}
		public static void SaveSettings()
		{
			try
			{
				// first save default user options
				SaveUserOptions();

				XmlNode activeNode;
				XmlNode rootNode;
				XmlNode node;
				XmlAttribute attribute;
				XmlDocument xmlDoc = new XmlDocument();

				xmlDoc.LoadXml("<?xml version='1.0' encoding='utf-8' ?><Configurations></Configurations>");
				rootNode = xmlDoc.SelectSingleNode("Configurations");

				// providers -------
				_providers.SaveToXml(xmlDoc, rootNode);

				// webData -------
				_webData.SaveToXml(xmlDoc, rootNode);

				// authentication ------
				_authentication.SaveToXml(xmlDoc, rootNode);

				// autoUpdate -------
				_autoUpdate.SaveToXml(xmlDoc, rootNode);

				// logSystem -------
				_logSystem.SaveToXml(xmlDoc, rootNode);

				// netProxy -------
				_netProxy.SaveToXml(xmlDoc, rootNode);

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
				// first read default user options
				ReadUserOptions();

				XmlNode activeNode;
				XmlNode rootNode;
				XmlDocument xmlDoc = new XmlDocument();

				// load the assembly file
				xmlDoc.Load(ConfigurationsFile);

				rootNode = xmlDoc.SelectSingleNode("Configurations");

				// providers -------
				_providers.ReadFromXml(rootNode);

				// webData -------
				_webData.ReadFromXml(rootNode);

				// authentication ------
				_authentication.ReadFromXml(rootNode);

				// authentication ------
				_imageCompressor.ReadFromXml(rootNode);

				// autoUpdate -------
				_autoUpdate.ReadFromXml(rootNode);

				// logSystem -------
				_logSystem.ReadFromXml(rootNode);

				// netProxy -------
				_netProxy.ReadFromXml(rootNode);

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
			catch (Exception) { }
		}
		private static void ReadUserOptions()
		{
			string config = ConfigurationManager.AppSettings["DefaultUserOptionsXml"];
			if (HttpContext.Current != null)
				config = HttpContext.Current.Server.MapPath(config);
			_userOptions = UserOptions.ReadFromXml(config);
		}
		private static void SaveUserOptions()
		{
			string config = ConfigurationManager.AppSettings["DefaultUserOptionsXml"];
			if (HttpContext.Current != null)
				config = HttpContext.Current.Server.MapPath(config);
			_userOptions.SaveToXml(config);
		}
		#endregion

		#region config classes
		private interface IConfigSection
		{
			void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode);
			void ReadFromXml(XmlNode rootNode);
		}
		public struct ProvidersConfig : IConfigSection
		{
			public bool EngineCanBeOverwritten;
			public bool PluginsEnabled;

			public void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode)
			{
				XmlNode node;
				XmlAttribute attribute;

				XmlNode activeNode = xmlDoc.CreateElement("providers");
				rootNode.AppendChild(activeNode);

				node = xmlDoc.CreateElement("engine");
				activeNode.AppendChild(node);
				attribute = xmlDoc.CreateAttribute("canBeOverwritten");
				attribute.Value = this.EngineCanBeOverwritten.ToString();
				node.Attributes.Append(attribute);

				node = xmlDoc.CreateElement("plugins");
				activeNode.AppendChild(node);
				attribute = xmlDoc.CreateAttribute("enabled");
				attribute.Value = this.PluginsEnabled.ToString();
				node.Attributes.Append(attribute);
			}
			public void ReadFromXml(XmlNode rootNode)
			{
				XmlNode node = rootNode.SelectSingleNode("providers/engine");
				this.EngineCanBeOverwritten = Convert.ToBoolean(node.Attributes["canBeOverwritten"].Value);

				node = rootNode.SelectSingleNode("providers/plugins");
				this.PluginsEnabled = Convert.ToBoolean(node.Attributes["enabled"].Value);
			}
		}

		public struct WebDataConfig : IConfigSection
		{
			public long MaxContentLength;
			public int RequestTimeout;
			public int RequestReadWriteTimeOut;
			public bool SendSignature;
			public string PreferredLocalEncoding;
			public UserAgentMode UserAgent;
			public string UserAgentCustom;

			public void SaveToXml(XmlDocument xmlDoc, XmlNode rootNode)
			{
				XmlAttribute attribute;

				XmlNode node = xmlDoc.CreateElement("webData");
				rootNode.AppendChild(node);


				attribute = xmlDoc.CreateAttribute("maxContentLength");
				attribute.Value = this.MaxContentLength.ToString();
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("requestTimeout");
				attribute.Value = this.RequestTimeout.ToString();
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("requestReadWriteTimeOut");
				attribute.Value = this.RequestReadWriteTimeOut.ToString();
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("sendSignature");
				attribute.Value = this.SendSignature.ToString();
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("preferredLocalEncoding");
				attribute.Value = this.PreferredLocalEncoding;
				node.Attributes.Append(attribute);

				XmlNode nodeUserAgent = xmlDoc.CreateElement("userAgent");
				node.AppendChild(nodeUserAgent);
				node = nodeUserAgent;

				attribute = xmlDoc.CreateAttribute("mode");
				attribute.Value = this.UserAgent.ToString();
				node.Attributes.Append(attribute);

				attribute = xmlDoc.CreateAttribute("custom");
				attribute.Value = this.UserAgentCustom;
				node.Attributes.Append(attribute);


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

				node = node.SelectSingleNode("userAgent");
				this.UserAgent = (UserAgentMode)Enum.Parse(typeof(UserAgentMode), node.Attributes["mode"].Value, true);
				this.UserAgentCustom = node.Attributes["custom"].Value;
			}

			public enum UserAgentMode { Default, ASProxy, Custom };
		}

		public struct AuthenticationConfig : IConfigSection
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

				if (this.Enabled && this.Users != null && this.Users.Count > 0)
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
				XmlNode node = rootNode.SelectSingleNode("authentication"); ;

				this.Enabled = Convert.ToBoolean(node.Attributes["enabled"].Value);
				if (this.Enabled)
				{
					this.Users = new List<AuthenticationConfig.User>();
					foreach (XmlNode childNode in node.ChildNodes)
					{
						AuthenticationConfig.User user = new AuthenticationConfig.User();
						user.UserName = childNode.Attributes["userName"].Value;
						user.Password = childNode.Attributes["password"].Value;
						user.Pages = Convert.ToBoolean(childNode.Attributes["pages"].Value);
						user.Images = Convert.ToBoolean(childNode.Attributes["images"].Value);
						user.Downloads = Convert.ToBoolean(childNode.Attributes["downloads"].Value);
						this.Users.Add(user);
					}
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

		public struct ImageCompressorConfig : IConfigSection
		{
			public bool Enabled;
			public long Quality;

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
				this.Quality = Convert.ToInt64(node.Attributes["quality"].Value);
			}
		}
		public struct AutoUpdateConfig : IConfigSection
		{
			public string UpdateInfoUrl;
			public bool Engine;
			public bool Plugins;

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
		public struct LogSystemConfig : IConfigSection
		{
			public int MaxFileSize;
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
				this.MaxFileSize = Convert.ToInt32(node.Attributes["maxFileSize"].Value);

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

		public struct NetProxyConfig : IConfigSection
		{
			private IWebProxy _webProxy;
			public NetProxyMode Mode;
			public string Address;
			public int Port;
			public bool Authentication;
			public string Authentication_Username;
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
									node.Attributes["mode"].Value,true);
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

			public enum NetProxyMode { Direct, SystemDefault, Custom }
		}
		#endregion
	}
}