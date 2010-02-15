using System;
using System.Collections.Specialized;
using System.Web;

namespace SalarSoft.ASProxy
{
	public class UrlProvider
	{
		public static void ParseASProxyPageUrl(NameValueCollection collection, out bool decode, out string url)
		{
			decode = true;
			url = "";
			try
			{
				string str = collection[Consts.Query.UrlAddress];
				if (!string.IsNullOrEmpty(str))
				{
					url = str;
					object obj = collection[Consts.Query.Decode];
					if (obj != null)
					{
						try
						{
							decode = Convert.ToBoolean(Convert.ToInt32(obj));
						}
						catch
						{
							decode = false;
						}
					}
					else
						decode = false;
				}
			}
			catch
			{
				url = "";
				decode = false;
			}
		}

		/// <summary>
		/// Generates ASProxy navigation URL
		/// </summary>
		public static string GetASProxyPageUrl(string asproxyPage, string url, bool encodeUrl)
		{
			if (encodeUrl)
				url = EncodeUrl(url);
			else
				url = UrlProvider.EscapeUrlQuery(url);

			return String.Format("{0}?{1}={2}&{3}={4}",
					   asproxyPage,
					   Consts.Query.Decode,
					   Convert.ToSByte(encodeUrl),
					   Consts.Query.UrlAddress,
					   url);
		}

		/// <summary>
		/// ASProxy base path.
		/// For http://site.com/asproxy/surf.aspx the result will be http://site.com/asproxy/
		/// </summary>
		/// <returns>For http://site.com/asproxy/surf.aspx the result will be http://site.com/asproxy/</returns>
		public static string GetAppAbsolutePath()
		{
			HttpRequest request = HttpContext.Current.Request;
			string result = request.Url.Scheme + "://" + request.Url.Authority + request.ApplicationPath;

			// BUGFIX 5.1: app path should end with slash character (/)
			// Occurs only with subdirectories.
			char last = result[result.Length - 1];
			if (last != '\\' && last != '/')
				result += '/';

			return result;
		}

		/// <summary>
		/// ASProxy site root path. 
		/// For http://site.com/asproxy/surf.aspx the result will be http://site.com
		/// </summary>
		/// <returns>For http://site.com/asproxy/surf.aspx the result will be http://site.com</returns>
		public static string GetAppAbsoluteBasePath()
		{
			HttpRequest request = HttpContext.Current.Request;
			return request.Url.Scheme + "://" + request.Url.Authority;
		}

		/// <summary>
		/// Gets url page full path without parameter queries
		/// </summary>
		/// <returns>e.g. http://www.site.com/dir1/dir2/page.ht?hi=1 returns: http://www.site.com/dir1/dir2/page.ht</returns>
		public static string GetPageAbsolutePath(string url)
		{
			Uri address = new Uri(url);
			return address.Scheme + "://" + address.Authority + address.AbsolutePath;
		}

		/// <summary>
		/// Gets url page full path without parameter queries
		/// </summary>
		/// <returns>e.g. http://www.site.com/dir1/dir2/page.ht?hi=1 returns: http://www.site.com/dir1/dir2/page.ht</returns>
		public static string GetPageAbsolutePath(Uri url)
		{
			return url.Scheme + "://" + url.Authority + url.AbsolutePath;
		}

		/// <summary>
		/// Gets URL site base path. e.g. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/
		/// </summary>
		/// <returns>e.g. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/</returns>
		public static string GetRootPath(string url)
		{
			Uri uri = new Uri(url);
			return uri.Scheme + "://" + uri.Authority + uri.Segments[0];
		}

		/// <summary>
		/// Gets URL site base path. e.g. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/
		/// </summary>
		/// <returns>e.g. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/</returns>
		public static string GetRootPath(Uri uri)
		{
			return uri.Scheme + "://" + uri.Authority + uri.Segments[0];
		}

		/// <summary>
		/// Gets URL site base path. E.G. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/dir1/dir2/
		/// </summary>
		/// <returns>E.G. http://www.site.com/dir1/dir2/page.ht?hi=1 returns only: http://www.site.com/dir1/dir2/</returns>
		public static string GetPagePath(string url)
		{
			Uri address = new Uri(url);
			string result = address.Scheme + "://" + address.Authority;

			// Don't add last segment
			for (int i = 0; i < address.Segments.Length - 1; i++)
			{
				// adding segments
				result += address.Segments[i];
			}

			if (address.Segments.Length > 0)
			{
				// add if only the last segment is a directory!!!!!
				string last = address.Segments[address.Segments.Length - 1];

				// is a directory?
				if (last[last.Length - 1] == '/')
					result += last;
			}

			// the end slash ensure
			// unnecessary, but needed for maintenance
			if (result[result.Length - 1] != '/')
				result += '/';

			return result;
		}

		/// <summary>
		/// Adds missing Http protocol to the url
		/// </summary>
		public static string CorrectInputUrl(string url)
		{
			if (url.IndexOf("://") == -1)
				url = "http://" + url;
			return url;
		}

		/// <summary>
		/// Checks if the request to ASProxy has url addess of back-end site.
		/// </summary>
		public static bool IsASProxyAddressUrlIncluded(NameValueCollection queryColl)
		{
			string str = queryColl[Consts.Query.UrlAddress];
			if (string.IsNullOrEmpty(str))
				return false;
			else
				return true;
		}


		/// <summary>
		/// List of client side url prefixes
		/// </summary>
		static string[] _ClientSideUrls = new string[] { "mailto:", "file://", "javascript:", "vbscript:", "jscript:", "vbs:", "ymsgr:", "data:" };
		/// <summary>
		/// Checks if the specified url is client site url
		/// </summary>
		public static bool IsClientSitdeUrl(string path)
		{
			path = path.ToLower();
			for (int i = 0; i < _ClientSideUrls.Length; i++)
			{
				// MatchCase test is faster
				if (StringCompare.StartsWithMatchCase(ref path, _ClientSideUrls[i]))
					return true;
			}
			return false;
		}

		/// <summary>
		/// List of internet protocols and url prefixes
		/// </summary>
		static string[] _NonVirtualUrls = new string[] { "http://", "https://", "mailto:", "ftp://", "file://", "telnet://", "news://", "nntp://", "ldap://", "ymsgr:", "javascript:", "vbscript:", "jscript:", "vbs:", "data:" };
		/// <summary>
		/// Checks if the specified url is virtual
		/// </summary>
		public static bool IsVirtualUrl(string path)
		{
			path = path.ToLower();
			for (int i = 0; i < _NonVirtualUrls.Length; i++)
			{
				// MatchCase test is faster
				if (StringCompare.StartsWithMatchCase(ref path, _NonVirtualUrls[i]))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Checks if specified url is javascript code or not
		/// </summary>
		public static bool IsJavascriptUrl(string path)
		{
			if (StringCompare.StartsWithMatchCase(ref path, "javascript:"))
				return true;
			return false;
		}

		/// <summary>
		/// Checks if specified url is ftp or not
		/// </summary>
		public static bool IsFTPUrl(string url)
		{
			if (StringCompare.StartsWithIgnoreCase(ref url, "ftp://"))
				return true;
			return false;
		}

		/// <summary>
		/// Removes tab,line feed, new line characters from html Url
		/// </summary>
		public static string IgnoreInvalidUrlCharctersInHtml(string htmlUrl)
		{
			htmlUrl = htmlUrl.Replace("\n", string.Empty);
			htmlUrl = htmlUrl.Replace("\r", string.Empty);
			htmlUrl = htmlUrl.Replace("\t", string.Empty);
			htmlUrl = htmlUrl.Replace("/./", "/");
			return htmlUrl;
		}

		/// <summary>
		/// Add a slash to end of url addresses. http://google.com --> http://google.com/
		/// </summary>
		/// <returns>Slash ended url</returns>
		public static string AddSlashToEnd(string url)
		{
			if (url.Length > 0)
			{
				if (url[url.Length - 1] != '\\' && url[url.Length - 1] != '/')
				{
					return url + '/';
				}
			}
			return url;
		}

		/// <summary>
		/// Joins two part of url. Path1 will be ignored if path2 is not virtual url.
		/// </summary>
		public static string JoinUrl(string path1, string path2)
		{
			if (!IsVirtualUrl(path2))
				return path1;

			path1 = AddSlashToEnd(path1);

			if (path2.Length > 0)
			{
				if (path2[0] == '/')
					return path1 + '.' + path2;
			}

			return path1 + path2;
		}

		/// <summary>
		/// Joins url parts
		/// </summary>
		public static string JoinUrl(string query, string pageUrlWithoutParameters, string pageBasePath, string siteBasePath)
		{
			// chiching page parameter
			// BUG: sometimes the query is empty, se the result should also include page queries
			// This may cause some bugs
			if (query.Length == 0)
				return pageUrlWithoutParameters;

			// If the query contains only parameter
			if (query[0] == '?')
			{
				//string pageUrlWithoutParameters = UrlProvider.GetPageAbsolutePath(pageUrl);
				return UrlBuilder.AppendAntoherQueries(pageUrlWithoutParameters, query);// Safe but slow
			}

			string tmpPage = query.ToLower();

			// check if the url is a foreign site url like this: http://www.google.com
			if (!IsVirtualUrl(tmpPage))
				return query;
			else
			{
				pageBasePath = AddSlashToEnd(pageBasePath);
				siteBasePath = AddSlashToEnd(siteBasePath);

				// if "page" is a base url like this: /Test/default.aspx
				if (query[0] == '/')
					return siteBasePath + '.' + query;

				// if "page" is a virtual url like this: SalarSoft/default.aspx
				return pageBasePath + query;
			}
		}



		/// <summary>
		/// Decode url address from unknown to know mode
		/// </summary>
		/// <param name="url">An encoded url address</param>
		public static string DecodeUrl(string url)
		{
			string result = url;
			int pos = url.LastIndexOf(Consts.Query.Base64Unknowner);
			if (pos != -1)
			{
				// After Base64Unknowner nothing is needed, so we can remove them
				// This can fix lots of javascript generated URLs bad behaviour
				// Since v5.0
				result = result.Substring(0, pos);
			}

			// BUGFIX: Base64 hash algorithm encodes (~) chracter to (+) and browsers indicates that this is a space!!
			// So, I have to replace spaces with (+) character
			// Since v5.0
			result = result.Replace(' ', '+');

			result = UrlEncoders.DecodeFromASProxyBase64(result);
			return result;
		}

		/// <summary>
		/// Encode url address to make it unknown
		/// </summary>
		/// <param name="url">A url address</param>
		public static string EncodeUrl(string url)
		{
			string result = UrlEncoders.EncodeToASProxyBase64(url);
			result += Consts.Query.Base64Unknowner;
			return result;
		}

		/// <summary>
		/// Encodes only a few special characters, to make the url safe for another query.
		/// Only "?" , "&amp;" and "#" characters.
		/// </summary>
		public static string EscapeUrlQuery(string url)
		{
			string result = Common.ReplaceStrEx(url, "?", "%3F", StringComparison.Ordinal);
			result = Common.ReplaceStrEx(result, "&", "%26", StringComparison.Ordinal);
			result = Common.ReplaceStrEx(result, "#", "%23", StringComparison.Ordinal);
			return result;
		}


		public static string SwitchToDirSep(string str)
		{
			return str.Replace('/', '\\');
		}
		public static string SwitchToUrlSep(string str)
		{
			return str.Replace('\\', '/');
		}


		/// <summary>
		/// Use this method for file addresses ,only. This method may be use in debug mode and is useless in the web!
		/// </summary>
		/// <returns>Exam: file://h:\dir1\dir2/dir3/file.zip --> file://h:\dir1\dir2\dir3\file.zip</returns>
		public static string CorrectFileUrl(string fileUrl)
		{
			string result = fileUrl;
			if (fileUrl.ToLower().IndexOf("file://") != -1)
			{
				result = UrlProvider.SwitchToDirSep(result);
				result = result.Remove(0, @"file:\\".Length);
				result = "file://" + result;
			}
			return result;
		}

		public static bool GetRequestQuery(NameValueCollection collection, string key, out string result)
		{
			string r = collection[key];
			if (!string.IsNullOrEmpty(r))
			{
				result = r;
				return true;
			}
			else
			{
				result = "";
				return false;
			}
		}
	}
}