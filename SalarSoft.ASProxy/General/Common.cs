using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SalarSoft.ASProxy
{
    /// <summary>
    /// Genereal functions for public use
    /// </summary>
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

        /// <summary>
        /// Checks if the application is running on dotNetFramework 4.0
        /// </summary>
        public static bool IsRunningOnDotNet4()
        {
			return Environment.Version.Major == 4;
        }

		/// <summary>
		/// Compares ASProxy version numbers, (5.5 > 5.5b1) and (5.5b3 > 5.5b1) and so on.
		/// The results would be (version1>version2)==1 , (version2>version1)==-1 , (version1=version2)==0
		/// </summary>
		public static int CompareASProxyVersions(string version1, string version2)
		{
			version1 = version1.ToLower();
			version2 = version2.ToLower();
			if (version1.Length > version2.Length)
			{
				string ver1 = version1.Substring(0, version2.Length);
				int compare = string.Compare(ver1, version2);

				// if they are same
				if (compare == 0)
				{
					if (version1[version2.Length] == 'b' || version1[version2.Length] == 'a')
						return -1; // version 2 is bigger
					else
						return 1;
				}
				else
					return compare;

			}
			else if (version1.Length == version2.Length)
			{
				return string.Compare(version1, version2);
			}
			else
			{
				string ver2 = version2.Substring(0, version1.Length);
				int compare = string.Compare(version1, ver2);

				// if they are same
				if (compare == 0)
				{
					if (version2[version1.Length] == 'b' || version2[version1.Length] == 'a')
						return 1; // version 1 is bigger
					else
						return -1;
				}
				else
					return compare;
			}
		}

		/// <summary>
		/// List of installed languages, the collection will contain their culture names.
		/// The method uses the installed ResX files in App_LocalResources folder to generate the list.
		/// </summary>
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

		/// <summary>
		/// Retrieve http-status code for the specified exception, default is InternalServerError(500).
		/// Returns negative error number if error is WebException but is not Http error.
		/// </summary>
		/// <returns>Integer number</returns>
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

		/// <summary>
		/// Retrieve http-status code for the specified exception, default is InternalServerError(500)
		/// </summary>
		/// <returns>HttpStatusCode</returns>
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

		/// <summary>
		/// Generares ASProxy usergaent for browsers,
		/// it will have asproxy signature in the of user-agent string
		/// </summary>
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
		/// Implements fast string replacing algorithm for CS
		/// </summary>
		public static string ReplaceStrEx(string original, string pattern, string replacement, StringComparison comparisonType)
		{
			if (original == null)
			{
				return null;
			}

			if (String.IsNullOrEmpty(pattern))
			{
				return original;
			}

			int lenPattern = pattern.Length;
			int idxPattern = -1;
			int idxLast = 0;

			StringBuilder result = new StringBuilder();

			while (true)
			{
				idxPattern = original.IndexOf(pattern, idxPattern + 1, comparisonType);

				if (idxPattern < 0)
				{
					result.Append(original, idxLast, original.Length - idxLast);

					break;
				}

				result.Append(original, idxLast, idxPattern - idxLast);
				result.Append(replacement);

				idxLast = idxPattern + lenPattern;
			}

			return result.ToString();
		}

        /// <summary>
        /// Generates string of a name-value collection
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

		/// <summary>
		/// Renames ASP.NET VIEW_STATE input field name to some more secure name
		/// </summary>
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

		/// <summary>
		/// Renames ASP.NET VIEW_STATE input field name to some more secure name
		/// </summary>
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

		/// <summary>
		/// Restores renamed VIEW_STATE input field name
		/// </summary>
        public static string AspDotNetViewStateResetToDef(string query)
        {
            return query.Replace(
                Consts.DataProccessing.ASPDotNETRenamedViewState,
                Consts.DataProccessing.ASPDotNETViewState);
        }

        /// <summary>
        /// Clears response header whitout harming http-compression header
        /// </summary>
        public static void ClearHeadersButSaveEncoding(HttpResponse response)
        {
            HttpCookie cookie = response.Cookies[Consts.FrontEndPresentation.HttpCompressor];
            string encode = cookie[Consts.FrontEndPresentation.HttpCompressEncoding];
            response.ClearHeaders();

            if (string.IsNullOrEmpty(encode))
                return;
            response.AppendHeader("Content-Encoding", encode);
        }

		/// <summary>
		/// Converts contentType to DataTypeToProcess
		/// </summary>
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

		/// <summary>
		/// Converts content type enum value to HTTP content-type header value
		/// </summary>
        public static string ContentTypeToString(MimeContentType type)
        {
            if (type == MimeContentType.application)
                return "application/octet-stream";
            return type.ToString().Replace('_', '/');
        }

		/// <summary>
		/// Converts HTTP Content-Type to enum type
		/// </summary>
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

		/// <summary>
		/// Base64 decoder
		/// </summary>
		public static string ConvertFromBase64(string B64String)
        {
            string result;

            byte[] buff = Convert.FromBase64String(B64String);
            result = UTF8Encoding.UTF8.GetString(buff);
            return result;
        }

		/// <summary>
		/// Base64 encoder
		/// </summary>
		public static string ConvertToBase64(string str)
        {
            string result;
            byte[] bytes = UTF8Encoding.UTF8.GetBytes(str);
            result = Convert.ToBase64String(bytes);
            return result;
        }

        /// <summary>
        /// Performs the ROT13 character rotation.
        /// </summary>
        //public static string Rot13Transform(string value)
        //{
        //    char[] array = value.ToCharArray();
        //    for (int i = 0; i < array.Length; i++)
        //    {
        //        int number = (int)array[i];
        //        if (number >= 'a' && number <= 'z')
        //        {
        //            if (number > 'm')
        //            {
        //                number -= 13;
        //            }
        //            else
        //            {
        //                number += 13;
        //            }
        //        }
        //        else if (number >= 'A' && number <= 'Z')
        //        {
        //            if (number > 'M')
        //            {
        //                number -= 13;
        //            }
        //            else
        //            {
        //                number += 13;
        //            }
        //        }
        //        array[i] = (char)number;
        //    }
        //    return new string(array);
        //}
        #endregion
    }
}