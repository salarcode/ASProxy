using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SalarSoft.ASProxy
{
	public class Common
	{
		#region public static

		/// <summary>
		/// Checks if the application is running on Mono framework
		/// </summary>
		public static bool IsRunningOnMono()
		{
			return Type.GetType("Mono.Runtime") != null;
		}

		public static NameValueCollection GetInstalledLanguagesList()
		{
			NameValueCollection languages = new NameValueCollection();


			string[] list = Directory.GetFiles(HttpContext.Current.Server.MapPath("~/App_LocalResources"), "*.resx", SearchOption.AllDirectories);
			foreach (string str in list)
			{
				Regex regex = new Regex("[a-zA-Z-]*.resx");
				string language = regex.Match(str).ToString().Replace(".resx", String.Empty);

				CultureInfo cultureInfo;
				try
				{
					if (language == "aspx")
					{
						cultureInfo = CultureInfo.GetCultureInfo("en-US");
					}
					else
					{
						cultureInfo = CultureInfo.GetCultureInfo(language);
					}
				}
				catch
				{
					// just ignore it
					continue;
				}

				languages[cultureInfo.IetfLanguageTag.ToLower()] = cultureInfo.NativeName;
			}
			return languages;
		}

		public static int GetExceptionHttpDetailedErrorCode(Exception exception)
		{
			if (exception != null)
			{
				if (exception is WebException)
				{
					WebException webEx = (WebException)exception;
					if (webEx.Response != null && webEx.Response is HttpWebResponse)
					{
						return (int)((HttpWebResponse)webEx.Response).StatusCode;
					}
					else
					{
						// Added to save more info
						return -(int)webEx.Status;
					}
				}
				else if (exception is HttpException)
				{
					return (int)((HttpStatusCode)((HttpException)exception).GetHttpCode());
				}
			}
			return (int)HttpStatusCode.InternalServerError;
		}

		public static HttpStatusCode GetExceptionHttpErrorCode(Exception exception)
		{
			if (exception != null)
			{
				if (exception is WebException)
				{
					WebException webEx = (WebException)exception;
					if (webEx.Response != null && webEx.Response is HttpWebResponse)
					{
						return ((HttpWebResponse)webEx.Response).StatusCode;
					}
				}
				else if (exception is HttpException)
				{
					return (HttpStatusCode)((HttpException)exception).GetHttpCode();
				}
			}
			return HttpStatusCode.InternalServerError;
		}

		public static string GenerateUserAgent(HttpContext context)
		{
			string userAgent;
			if (context != null)
				userAgent = context.Request.UserAgent;
			else
				return Consts.BackEndConenction.ASProxyUserAgent;

			if (string.IsNullOrEmpty(userAgent))
				return Consts.BackEndConenction.ASProxyUserAgent;

			return userAgent.Trim() + " " + Consts.BackEndConenction.ASProxyAgentVersion;
		}


		/// <summary>
		/// Get string of a query collection
		/// </summary>
		public static string NameValueCollectionToString(NameValueCollection queryCollection)
		{
			string result = string.Empty;
			for (int i = 0; i < queryCollection.Count; i++)
			{
				result += queryCollection.GetKey(i) + "=" + queryCollection.Get(i);
				if (i < queryCollection.Count - 1)
					result += "&";
			}
			return result;
		}

		public static byte[] AspDotNetViewStateResetToDef(Stream src)
		{
			byte[] result;
			try
			{
				result = new byte[src.Length];
				src.Read(result, 0, result.Length);
			}
			catch (NotSupportedException)
			{
				// If reading directly nor supported
				ArrayList arrayBuff = new ArrayList();
				byte[] buff = new byte[256];
				int readed = 0;
				do
				{
					readed = src.Read(buff, 0, buff.Length);
					for (int i = 0; i < readed; i++)
					{
						arrayBuff.Add(buff[i]);
					}
				}
				while (readed > 0);
				result = (byte[])arrayBuff.ToArray(typeof(byte));
				arrayBuff.Clear();
			}

			result = ApplyViewStateResetRename(result);
			return result;
		}

		public static byte[] ApplyViewStateResetRename(byte[] buff)
		{
			string str = Encoding.ASCII.GetString(buff);
			int pos = str.IndexOf(Consts.DataProccessing.ASPDotNETRenamedViewState);
			if (pos != -1)
			{
				// Add frist part before ViewState 
				byte[] result = new byte[buff.Length + (Consts.DataProccessing.ASPDotNETViewState.Length - Consts.DataProccessing.ASPDotNETRenamedViewState.Length)];
				for (int i = 0; i < pos; i++)
					result[i] = buff[i];

				// Add new ViewState 
				byte[] aspDotNET = Encoding.ASCII.GetBytes(Consts.DataProccessing.ASPDotNETViewState);

				for (int i = 0; i < aspDotNET.Length; i++)
					result[i + pos] = aspDotNET[i];


				// Add third part after ViewState
				int diff = Consts.DataProccessing.ASPDotNETRenamedViewState.Length - Consts.DataProccessing.ASPDotNETViewState.Length;
				for (int i = pos + aspDotNET.Length; i < result.Length; i++)
					result[i] = buff[i + diff];

				//string s = Encoding.ASCII.GetString(result);
				return result;

			}
			else
				return buff;
		}


		public static string AspDotNetViewStateResetToDef(string query)
		{
			return query.Replace(
				Consts.DataProccessing.ASPDotNETRenamedViewState,
				Consts.DataProccessing.ASPDotNETViewState);
		}

		/// <summary>
		/// Clears response header then recovers compression header
		/// </summary>
		public static void ClearHeadersButSaveEncoding(HttpResponse response)
		{
			HttpCookieCollection cool = response.Cookies;

			HttpCookie cookie = response.Cookies[Consts.FrontEndPresentation.HttpCompressorCookieName];
			string encode = cookie[Consts.FrontEndPresentation.HttpCompressEncoding];
			response.ClearHeaders();

			if (string.IsNullOrEmpty(encode))
				return;

			response.AppendHeader("Content-Encoding", encode);
		}

		/// <summary>
		/// Is this url a FTP url
		/// </summary>
		public static bool IsFTPUrl(string url)
		{
			return url.Trim().StartsWith("ftp://", StringComparison.CurrentCultureIgnoreCase);
		}

		public static DataTypeToProcess MimeTypeToToProcessType(MimeContentType mime)
		{
			switch (mime)
			{
				case MimeContentType.text_html:
					return DataTypeToProcess.Html;

				case MimeContentType.text_css:
					return DataTypeToProcess.Css;

				case MimeContentType.text_javascript:
					return DataTypeToProcess.JavaScript;

				case MimeContentType.text_plain:
				case MimeContentType.image_jpeg:
				case MimeContentType.image_gif:
				case MimeContentType.application:
					return DataTypeToProcess.None;
				default:
					return DataTypeToProcess.None;
			}
		}

		public static string ContentTypeToString(MimeContentType type)
		{
			if (type == MimeContentType.application)
				return "application/octet-stream";
			return type.ToString().Replace('_', '/');
		}

		public static MimeContentType StringToContentType(string type)
		{
			if (string.IsNullOrEmpty(type))
				return MimeContentType.application;

			type = type.ToLower();

			string[] parts = type.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length > 0)
				type = parts[0];

			switch (type)
			{
				case "text/html":
					return MimeContentType.text_html;
				case "text/plain":
					return MimeContentType.text_plain;
				case "text/css":
					return MimeContentType.text_css;
				case "text/javascript":
				case "text/jscript":
					return MimeContentType.text_javascript;
				case "image/jpeg":
					return MimeContentType.image_jpeg;
				case "image/gif":
					return MimeContentType.image_gif;
				case "application/octet-stream":
					return MimeContentType.application;
				default:
					{

						if (type.IndexOf("javascript") != -1)
							return MimeContentType.text_javascript;
						if (type.IndexOf("text") != -1)
							return MimeContentType.text_html;
						if (type.IndexOf("image") != -1)
							return MimeContentType.image_jpeg;
						if (type.IndexOf("application") != -1)
							return MimeContentType.application;
						return MimeContentType.application;
					}
			}
		}

		public static string FromBase64(string str)
		{
			string result;

			byte[] buff = Convert.FromBase64String(str);
			result = UTF8Encoding.UTF8.GetString(buff);
			return result;
		}

		public static string ToBase64(string B64String)
		{
			string result;
			byte[] bytes = UTF8Encoding.UTF8.GetBytes(B64String);
			result = Convert.ToBase64String(bytes);
			return result;
		}
		#endregion

	}
}