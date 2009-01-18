using System;
using System.Configuration;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

namespace SalarSoft.ASProxy
{
	public class ASProxyConfig
	{

		private static int _ASProxyServerPort = -1;
		private static string _ASProxyServerAdminPassword = null;
		private static int _WebProxyMode = -1;
		private static string _WebProxyHost = null;
		private static int _WebProxyPort = -1;
		private static IWebProxy _WebProxy = null;

		private static bool _ErrorLogEnabled = false;
		private static bool _LogSystemEnabled = false;
		private static bool _LogImagesEnabled = false;
		private static string _LogFilePath = null;

		private static string _ErrorLogFile = null;
		private static string _ASProxyLoginPassword = null;
		private static string _ASProxyLoginUser = null;
		private static bool? _ASProxyLoginNeeded = null;

		private static string _ASProxyAutoUpdateInfoUrl = null;
		private static bool? _ASProxyAutoUpdateEnabled = null;

		/// <summary>
		/// Default encoding for ASProxy
		/// </summary>
		public static readonly Encoding DefaultEncoding = Encoding.UTF8;

		static ASProxyConfig()
		{
			_ASProxyAutoUpdateEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["ASProxyAutoUpdateEnabled"]);
			_ASProxyAutoUpdateInfoUrl = ConfigurationManager.AppSettings["ASProxyAutoUpdateInfoUrl"];

			_ASProxyLoginNeeded = Convert.ToBoolean(ConfigurationManager.AppSettings["ASProxyLoginNeeded"]);
			_ASProxyLoginPassword = ConfigurationManager.AppSettings["ASProxyLoginPassword"];
			_ASProxyLoginUser = ConfigurationManager.AppSettings["ASProxyLoginUser"];

			_ASProxyServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["ASProxyServerPort"]);
			_ASProxyServerAdminPassword = ConfigurationManager.AppSettings["ASProxyServerAdminPassword"];
			_ErrorLogFile = ConfigurationManager.AppSettings["ErrorLogFile"];
			_ErrorLogEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["ErrorLogEnabled"]);

			_LogFilePath = ConfigurationManager.AppSettings["LogFilePath"];
			_LogSystemEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["LogSystemEnabled"]);
			_LogImagesEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["LogImagesEnabled"]);
		}

		public static bool ASProxyAutoUpdateEnabled
		{
			get
			{
				if (_ASProxyAutoUpdateEnabled.HasValue == false)
				{
					_ASProxyAutoUpdateEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["ASProxyAutoUpdateEnabled"]);
				}
				return _ASProxyAutoUpdateEnabled.Value;
			}
		}

		public static string ASProxyAutoUpdateInfoUrl
		{
			get
			{
				if (_ASProxyAutoUpdateInfoUrl == null)
				{
					_ASProxyAutoUpdateInfoUrl = ConfigurationManager.AppSettings["ASProxyAutoUpdateInfoUrl"];
				}
				return _ASProxyAutoUpdateInfoUrl;
			}
		}

		public static bool ASProxyLoginNeeded
		{
			get
			{
				if (_ASProxyLoginNeeded.HasValue == false)
				{
					_ASProxyLoginNeeded = Convert.ToBoolean(ConfigurationManager.AppSettings["ASProxyLoginNeeded"]);
				}
				return _ASProxyLoginNeeded.Value;
			}
		}

		public static string ASProxyLoginPassword
		{
			get
			{
				if (_ASProxyLoginPassword == null)
				{
					_ASProxyLoginPassword = ConfigurationManager.AppSettings["ASProxyLoginPassword"];
				}
				return _ASProxyLoginPassword;
			}
		}

		public static string ASProxyLoginUser
		{
			get
			{
				if (_ASProxyLoginUser == null)
				{
					_ASProxyLoginUser = ConfigurationManager.AppSettings["ASProxyLoginUser"];
				}
				return _ASProxyLoginUser;
			}
		}

		public static int ASProxyServerPort
		{
			get
			{
				if (_ASProxyServerPort == -1)
				{
					_ASProxyServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["ASProxyServerPort"]);
				}
				return _ASProxyServerPort;
			}
		}

		public static string ASProxyServerAdminPassword
		{
			get
			{
				if (_ASProxyServerAdminPassword == null)
				{
					_ASProxyServerAdminPassword = ConfigurationManager.AppSettings["ASProxyServerAdminPassword"];
				}
				return _ASProxyServerAdminPassword;
			}
		}

		public static string ErrorLogFile
		{
			get
			{
				if (_ErrorLogFile == null)
				{
					_ErrorLogFile = ConfigurationManager.AppSettings["ErrorLogFile"];
				}
				return _ErrorLogFile;
			}
		}

		public static bool ErrorLogEnabled
		{
			get
			{
				if (_ErrorLogFile == null)
				{
					string loadIt = ErrorLogFile;
					_ErrorLogEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["ErrorLogEnabled"]);
				}
				return _ErrorLogEnabled;
			}
		}

		public static bool LogSystemEnabled
		{
			get { return ASProxyConfig._LogSystemEnabled; }
			set { ASProxyConfig._LogSystemEnabled = value; }
		}

		public static bool LogImagesEnabled
		{
			get { return ASProxyConfig._LogImagesEnabled; }
			set { ASProxyConfig._LogImagesEnabled = value; }
		}

		public static string LogFilePath
		{
			get
			{
				if (_LogFilePath == null)
				{
					_LogFilePath = ConfigurationManager.AppSettings["LogFilePath"];
				}
				return _LogFilePath;
			}
		}


		public static bool IsWebProxyEnabled { get { return (WebProxyMode != 0 && WebProxyMode > 0); } }
		public static int WebProxyMode
		{
			get
			{
				if (_WebProxyMode == -1)
				{
					_WebProxyMode = Convert.ToInt32(ConfigurationManager.AppSettings["WebProxyMode"]);
				}
				return _WebProxyMode;
			}
		}

		public static string WebProxyHost
		{
			get
			{
				if (_WebProxyHost == null)
				{
					_WebProxyHost = ConfigurationManager.AppSettings["WebProxyHost"];
				}
				return _WebProxyHost;
			}
		}


		public static int WebProxyPort
		{
			get
			{
				if (_WebProxyPort == -1)
				{
					_WebProxyPort = Convert.ToInt32(ConfigurationManager.AppSettings["WebProxyPort"]);
				}
				return _WebProxyPort;
			}
		}

		public static IWebProxy GenerateWebProxy()
		{
			if (_WebProxy == null)
			{
				if (ASProxyConfig.IsWebProxyEnabled)
				{
					int mode = ASProxyConfig.WebProxyMode;
					switch (mode)
					{
						case 0://Disabled
							_WebProxy = new WebProxy();
							break;
						case 1://AutoDetect
							_WebProxy = WebProxy.GetDefaultProxy();
							break;
						case 2://Custom
							_WebProxy = new WebProxy(ASProxyConfig.WebProxyHost, ASProxyConfig.WebProxyPort);
							break;
					}
				}
			}
			return _WebProxy;
		}






		public static void SetCookieOptions(OptionsType opt)
		{
			HttpContext context = HttpContext.Current;
			if (context.Request.Browser.Cookies)
			{
				HttpCookie cookie = new HttpCookie(GlobalConsts.CookieMasterName);
				cookie.Expires = DateTime.Now.AddMonths(1);
				Type type = typeof(OptionsType);
				foreach (FieldInfo info in type.GetFields())
				{
					if ((bool)info.GetValue(opt) == true)
						cookie[info.Name] = "true";
					else
						cookie[info.Name] = "false";
				}
				context.Response.Cookies.Add(cookie);
			}
		}
		public static OptionsType GetCookieOptions()
		{
			OptionsType result = OptionsType.GetDefault(true);
			HttpContext context = HttpContext.Current;

			if (context.Request.Browser.Cookies)
			{
				HttpCookie cookie = context.Request.Cookies[GlobalConsts.CookieMasterName];
				if (cookie == null)
					return result;

				string cookievalue = "";
				object instance = (object)result;
				Type type = typeof(OptionsType);
				foreach (FieldInfo info in type.GetFields())
				{
					cookievalue = cookie[info.Name];

					// cookie has value?
					if (!string.IsNullOrEmpty(cookievalue))
					{
						try
						{
							info.SetValue(instance, Convert.ToBoolean(cookievalue));
						}
						catch
						{
							info.SetValue(instance, true);
						}
					}
				}
				result = (OptionsType)instance;
				return result;
			}
			else
			{
				return result;
			}
		}
	}


	public struct OptionsType
	{
		public bool AcceptCookies;
		public bool Images;
		public bool Scripts;
		public bool Links;
		public bool IFrame;
		public bool FrameSet;
		public bool BackImages;
		public bool SubmitForms;
		public bool CssLink;
		public bool EmbedObjects;
		public bool HttpCompression;
		public bool EncodeUrl;
		public bool DisplayOrginalUrl;
		public bool DisplayPageTitle;
		public bool IgnorePageEncoding;

		/// <summary>
		/// Inherite DOCTYPE.
		/// </summary>
		public bool DocType;

		/// <summary>
		/// Specifies scripts should strip or not. This option disabled by default.
		/// </summary>
		public bool RemoveScripts;

		/// <summary>
		/// Initialize a new instance of OptionsType with default value
		/// </summary>
		public static OptionsType GetDefault(bool val)
		{
			OptionsType result;
			result.HttpCompression = false;		// Disabled by default
			result.IgnorePageEncoding = false;	// Disabled by default
			result.RemoveScripts = false;		// Disabled by default
			result.BackImages = val;			// Disabled by default
			result.DocType = val;             // Disabled by default
			result.DisplayOrginalUrl = val;
			result.Images = val;
			result.Scripts = val;
			result.Links = val;
			result.IFrame = val;
			result.FrameSet = val;
			result.SubmitForms = val;
			result.CssLink = val;
			result.EncodeUrl = val;
			result.EmbedObjects = val;
			result.AcceptCookies = val;
			result.DisplayPageTitle = val;
			return result;
		}

	}

}
