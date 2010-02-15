using System.Web;
using System;
using System.Reflection;
using System.Xml;
namespace SalarSoft.ASProxy
{
	///<summary>
	/// User defined data processing options 
	///</summary>
	public struct UserOptions
	{
		private static UserOptions _defaultOptions;
		private bool _LoadedFromSource;

		public bool Cookies;
		public bool TempCookies;
		public bool Images;
		public bool Links;
		public bool Frames;
		public bool SubmitForms;
		public bool RemoveObjects;
		public bool HttpCompression;
		public bool ImageCompressor;
		public bool EncodeUrl;
		public bool OrginalUrl;
		public bool PageTitle;
		public bool ForceEncoding;
		/// <summary>
		/// Specifies scripts should strip or not. This option disabled by default.
		/// </summary>
		public bool RemoveScripts;
		public bool RemoveImages;
		/// <summary>
		/// Inherits DOCTYPE.
		/// </summary>
		public bool DocType;


		public bool LoadedFromSource
		{
			get { return _LoadedFromSource; }
			set { _LoadedFromSource = value; }
		}

		static UserOptions()
		{
			Configurations.UserOptionsConfig config = Configurations.UserOptions;

			_defaultOptions.Cookies = config.Cookies.Enabled;
			_defaultOptions.DocType = config.DocType.Enabled;
			_defaultOptions.EncodeUrl = config.EncodeUrl.Enabled;
			_defaultOptions.ForceEncoding = config.ForceEncoding.Enabled;
			_defaultOptions.Frames = config.Frames.Enabled;
			_defaultOptions.HttpCompression = config.HttpCompression.Enabled;
			_defaultOptions.ImageCompressor = config.ImageCompressor.Enabled;
			_defaultOptions.Images = config.Images.Enabled;
			_defaultOptions.Links = config.Links.Enabled;
			_defaultOptions.OrginalUrl = config.OrginalUrl.Enabled;
			_defaultOptions.PageTitle = config.PageTitle.Enabled;
			_defaultOptions.RemoveImages = config.RemoveImages.Enabled;
			_defaultOptions.RemoveObjects = config.RemoveObjects.Enabled;
			_defaultOptions.RemoveScripts = config.RemoveScripts.Enabled;
			_defaultOptions.SubmitForms = config.SubmitForms.Enabled;
			_defaultOptions.TempCookies = config.TempCookies.Enabled;

			_defaultOptions._LoadedFromSource = false;
		}

		/// <summary>
		/// Initialize a new instance of UserOptions with default value
		/// </summary>
		public static UserOptions LoadDefaults()
		{
			return _defaultOptions;
		}

		public void SaveToResponse()
		{
			HttpContext context = HttpContext.Current;

			HttpCookie cookie = new HttpCookie(Consts.FrontEndPresentation.UserOptionsCookieName);
			SaveToCookie(this, cookie);

			cookie.Expires = DateTime.Now.AddMonths(1);
			context.Response.Cookies.Add(cookie);
		}

		public static UserOptions ReadFromRequest()
		{
			UserOptions result;

			HttpContext context = HttpContext.Current;
			HttpCookie cookie = context.Request.Cookies[Consts.FrontEndPresentation.UserOptionsCookieName];
			if (cookie == null)
				return LoadDefaults();

			result = ReadFromCookie(cookie);
			result._LoadedFromSource = true;

			return result;
		}

		static UserOptions ReadFromCookie(HttpCookie cookie)
		{
			UserOptions result = LoadDefaults();

			result.Cookies = ConvertToBool(cookie["Cookies"], result.Cookies);
			result.DocType = ConvertToBool(cookie["DocT"], result.DocType);
			result.EncodeUrl = ConvertToBool(cookie["EncUrl"], result.EncodeUrl);
			result.ForceEncoding = ConvertToBool(cookie["PgEnc"], result.ForceEncoding);
			result.Frames = ConvertToBool(cookie["Frames"], result.Frames);
			result.HttpCompression = ConvertToBool(cookie["ZIP"], result.HttpCompression);
			result.ImageCompressor = ConvertToBool(cookie["ImgZip"], result.ImageCompressor);
			result.Images = ConvertToBool(cookie["Img"], result.Images);
			result.Links = ConvertToBool(cookie["Links"], result.Links);
			result.OrginalUrl = ConvertToBool(cookie["FloatBar"], result.OrginalUrl);
			result.PageTitle = ConvertToBool(cookie["Title"], result.PageTitle);
			result.RemoveImages = ConvertToBool(cookie["RemImg"], result.RemoveImages);
			result.RemoveObjects = ConvertToBool(cookie["RemObj"], result.RemoveObjects);
			result.RemoveScripts = ConvertToBool(cookie["RemScript"], result.RemoveScripts);
			result.SubmitForms = ConvertToBool(cookie["Forms"], result.SubmitForms);
			result.TempCookies = ConvertToBool(cookie["TempCookies"], result.TempCookies);

			return result;
		}
		static void SaveToCookie(UserOptions options, HttpCookie cookie)
		{
			cookie["Cookies"] = Convert.ToByte(options.Cookies).ToString();
			cookie["DocT"] = Convert.ToByte(options.DocType).ToString();
			cookie["EncUrl"] = Convert.ToByte(options.EncodeUrl).ToString();
			cookie["ForceEncode"] = Convert.ToByte(options.ForceEncoding).ToString();
			cookie["Frames"] = Convert.ToByte(options.Frames).ToString();
			cookie["ZIP"] = Convert.ToByte(options.HttpCompression).ToString();
			cookie["ImgZip"] = Convert.ToByte(options.ImageCompressor).ToString();
			cookie["Img"] = Convert.ToByte(options.Images).ToString();
			cookie["Links"] = Convert.ToByte(options.Links).ToString();
			cookie["Float"] = Convert.ToByte(options.OrginalUrl).ToString();
			cookie["Title"] = Convert.ToByte(options.PageTitle).ToString();
			cookie["RemImg"] = Convert.ToByte(options.RemoveImages).ToString();
			cookie["RemObj"] = Convert.ToByte(options.RemoveObjects).ToString();
			cookie["RemScript"] = Convert.ToByte(options.RemoveScripts).ToString();
			cookie["Forms"] = Convert.ToByte(options.SubmitForms).ToString();
			cookie["TempCookies"] = Convert.ToByte(options.TempCookies).ToString();
		}
		static bool ConvertToBool(string intValue, bool defaultValue)
		{
			try
			{
				if (!string.IsNullOrEmpty(intValue))
					return Convert.ToBoolean(Convert.ToInt16(intValue));
			}
			catch { }
			return defaultValue;
		}
	}
}