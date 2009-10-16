using System;
using System.Net;
using System.Web;
using System.Reflection;
using SalarSoft.ASProxy.Exposed;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace SalarSoft.ASProxy.BuiltIn
{


	/// <summary>
	/// Oh god, I could find the problem of cookies and it was damn microsoft's CookieContainer bug!
	/// I've reported this fucking bug, and it is mysteriously fixed in a day! 
	/// It is late now and 3 releases of .net have this bug, what should I do? Damn it!
	/// 
	/// 
	/// This class can work with bugless CookieContainer which arrives with .NET 4.0 .
	/// But I won't use it because of backward compatibility!
	/// 
	/// Sicim bo microsofte!
	/// </summary>
	public class CookieManager : ExCookieManager
	{
		protected const int intExpireDateNormalDays = 6;
		protected const int intExpireDateTempMinutes = 30;
		protected const string strCookieNameExt = "_ASPX";
		protected static readonly bool IsRunningOnMicrosoftCLR;

		static CookieManager()
		{
			IsRunningOnMicrosoftCLR = !Common.IsRunningOnMono();
		}

		/// <summary>
		/// Saves response cookies in a cookie collection
		/// </summary>
		/// <param name="httpResponse">proxy response instance</param>
		/// <param name="cookies">Specified cookie collection</param>
		public override void RestoreCookiesFromResponse(WebResponse httpResponse, CookieCollection cookies)
		{
			if (!(httpResponse is HttpWebResponse))
				return;

			if (cookies == null)
				return;

			HttpWebResponse webResponse = (HttpWebResponse)httpResponse;

			// Add 'em all
			cookies.Add(webResponse.Cookies);
		}

		/// <summary>
		/// Reads cookies from response and saves them all in user pc
		/// </summary>
		/// <param name="httpResponse">proxy response instance</param>
		public override void RestoreCookiesFromResponse(WebResponse webResponse, bool saveAsTemporary)
		{
			if (!(webResponse is HttpWebResponse) || HttpContext.Current == null)
				return;

			// Declarations
			HttpResponse userResponse = HttpContext.Current.Response;
			HttpRequest userRequest = HttpContext.Current.Request;

			HttpWebResponse httpWebResponse = (HttpWebResponse)webResponse;
			Uri responseUrl = httpWebResponse.ResponseUri;

			// if there is any cookie
			if (httpWebResponse.Cookies.Count > 0)
			{
				CookieContainer container = new CookieContainer();

				// if there is cookies 
				// restores cookies from user request
				ApplyRequestCookiesToCookieContainer(container, userRequest);

				// Response cookies
				CookieCollection responseCookies = httpWebResponse.Cookies;

				// Adding response cookies,
				// The new cookies will overwite the previous ones
				container.Add(responseCookies);

				// Only for Micosoft .NET Framework
				if (IsRunningOnMicrosoftCLR)
					// BUGFIX: CookieContainer has a bug, here is its bugfix
					// To get around this bug, the domains should start with a DOT
					BugFix_AddDotCookieDomain(container);

				// Get cookies which applies to the response url
				CookieCollection coll = container.GetCookies(responseUrl);

				// Cookies are grouped by thier domains
				Dictionary<string, CookieCollection> cookiesGroup = new Dictionary<string, CookieCollection>();

				// Sending cookies to the back-end user's browser
				foreach (Cookie cookie in coll)
				{
					// Get cookie name for current
					string cookieName = GetCookieNameByDomain_FrontEnd(cookie, responseUrl);
					string cookieHeader = GetCookieHeader(cookie);
					cookieHeader = HttpUtility.UrlEncode(cookieHeader);


					CookieCollection groupedCookie;
					if (cookiesGroup.TryGetValue(cookieName, out groupedCookie))
					{
						// the groups exists before

						groupedCookie.Add(cookie);
					}
					else
					{
						// new groups
						groupedCookie = new CookieCollection();
						groupedCookie.Add(cookie);

						// add to cookie groups
						cookiesGroup.Add(cookieName, groupedCookie);
					}
				}

				// cookies calculated timeout in minute
				DateTime expireDate;

				if (saveAsTemporary)
				{
					// only 30 minutes
					expireDate = DateTime.Now.AddMinutes(intExpireDateTempMinutes);
				}
				else
				{
					// To prevent cookie storage overflow, it should store cookies at most 6 days
					expireDate = DateTime.Now.AddDays(intExpireDateNormalDays);
				}

				// Add grouped cookies to front-end user
				foreach (KeyValuePair<string, CookieCollection> entry in cookiesGroup)
				{
					HttpCookie frontendCookie = new HttpCookie(entry.Key);

					// Header
					string cookiesHeader = GetCookieHeader(entry.Value);

					// Encode cookie header to make it safe
					cookiesHeader = HttpUtility.UrlEncode(cookiesHeader);

					// expire
					frontendCookie.Expires = expireDate;

					// Value
					frontendCookie.Value = cookiesHeader;

					// And finish...
					userResponse.Cookies.Add(frontendCookie);
				}
			}
		}

		/// <summary>
		/// Adds specified cookies to proxy request cookies collection
		/// </summary>
		/// <param name="httpRequest">Proxy request</param>
		/// <param name="cookies">Specified cookies</param>
		public override void AddCookiesToRequest(WebRequest httpRequest, CookieCollection cookies)
		{
			// if this is not a web request do nothing
			if (!(httpRequest is HttpWebRequest))
				return;
			HttpWebRequest webRequest = (HttpWebRequest)httpRequest;

			// Enable the cookies
			if (webRequest.CookieContainer == null)
				webRequest.CookieContainer = new CookieContainer();

			if (cookies == null)
				return;


			// Add whole cookie in collection
			//BUFIX: webRequest.CookieContainer.Add(webRequest.Address, cookies);
			webRequest.CookieContainer.Add(cookies);
		}

		/// <summary>
		/// Reads cookies from ASProxy cookies and adds them to request to site
		/// </summary>
		/// <param name="httpRequest">Proxy request</param>
		public override void AddCookiesToRequest(WebRequest webRequest)
		{
			// ---------------------------------------
			// Declarations
			if (!(webRequest is HttpWebRequest) || HttpContext.Current == null)
				return;

			HttpResponse userResponse = HttpContext.Current.Response;
			HttpRequest userRequest = HttpContext.Current.Request;
			if (userRequest == null || userResponse == null)
				return;
			HttpWebRequest httpWebRequest = (HttpWebRequest)webRequest;
			Uri webUri = httpWebRequest.Address;

			// ---------------------------------------
			// The code

			// Enable the cookies!
			if (httpWebRequest.CookieContainer == null)
				httpWebRequest.CookieContainer = new CookieContainer();


			// restore cookies from user request
			ApplyRequestCookiesToCookieContainer(httpWebRequest.CookieContainer, userRequest);

			// Only for Micosoft .NET Framework
			if (IsRunningOnMicrosoftCLR)
				// BUGFIX: CookieContainer has a bug
				// Here is its bugfix
				// To get around this bug, the domains should start with a DOT
				BugFix_AddDotCookieDomain(httpWebRequest.CookieContainer);

		}

		/// <summary>
		/// Reads request cookie to cookie container
		/// </summary>
		private void ApplyRequestCookiesToCookieContainer(CookieContainer container, HttpRequest userRequest)
		{
			if (container == null || userRequest == null)
				return;

			// if there is cookies 
			for (int i = 0; i < userRequest.Cookies.Count; i++)
			{
				HttpCookie cookie = userRequest.Cookies[i];
				if (IsASProxyCookie(cookie.Name))
				{
					string val;
					// BUGFIX: cookies header shouldn't be decoded
					//val = HttpUtility.UrlDecode(cookie.Value);
					val = cookie.Value;

					Uri host = GetASProxyCookieHost_New(cookie.Name);
					if (host != null)
					{
						// Tries to parse the cookie
						// Mono method does not work correctly with .NET
						// ParseAndAddCookies(val, host, container);

						 //BUG: SetCookies has a bug because it uses Add(cookie, url) which has a bug
						 container.SetCookies(host, val);
					}
				}
			}
		}

		private Uri GetASProxyCookieHost_New(string name)
		{
			int index = name.LastIndexOf(strCookieNameExt);
			if (index != -1)
			{
				// removing asproxy sign
				string host = name.Substring(0, index);

				// removing the beginning dot!
				if (host.StartsWith("."))
					host = host.Remove(0, 1);

				try
				{
					return new Uri("http://" + host);
				}
				catch (UriFormatException)
				{
					throw new UriFormatException("failed to parse: " + name);
				}
			}
			return null;
		}

		/// <summary>
		/// Parses the cookie header and adds the cookie to CookieContainer
		/// </summary>
		/// <remarks>
		/// Copied from Mono library
		/// </remarks>
		/// <see cref="http://anonsvn.mono-project.com/viewvc/trunk/mcs/class/System/System.Net/CookieContainer.cs?revision=140055&amp;view=markup"/>
		private void ParseAndAddCookies(string header, Uri uri, CookieContainer container)
		{
			if (header.Length == 0)
				return;

			string[] name_values = header.Trim().Split(';');
			int length = name_values.Length;
			Cookie cookie = null;
			int pos;

			CultureInfo inv = CultureInfo.InvariantCulture;

			bool havePath = false;
			bool haveDomain = false;

			for (int i = 0; i < length; i++)
			{
				pos = 0;
				string name_value = name_values[i].Trim();
				string name = ParseAndAddCookies_GetCookieName(name_value, name_value.Length, ref pos);
				if (name == null || name == "")
					throw new CookieException();//"Name is empty.");

				string value = ParseAndAddCookies_GetCookieValue(name_value, name_value.Length, ref pos);
				if (cookie != null)
				{
					if (!havePath && String.Compare(name, "$Path", true, inv) == 0 ||
						String.Compare(name, "path", true, inv) == 0)
					{
						havePath = true;
						cookie.Path = value;
						continue;
					}

					if (!haveDomain && String.Compare(name, "$Domain", true, inv) == 0 ||
						String.Compare(name, "domain", true, inv) == 0)
					{
						cookie.Domain = value;
						haveDomain = true;
						continue;
					}

					if (!havePath)
						cookie.Path = ParseAndAddCookies_GetDir(uri.AbsolutePath);

					if (!haveDomain)
						cookie.Domain = uri.Host;

					havePath = false;
					haveDomain = false;

					// Add to the contaier
					container.Add(cookie);
					cookie = null;
				}
				cookie = new Cookie(name, value);
			}

			if (cookie != null)
			{
				if (!havePath)
					cookie.Path = ParseAndAddCookies_GetDir(uri.AbsolutePath);

				// ERROR
				if (!haveDomain)
					cookie.Domain = uri.Host;

				// Add to the container
				container.Add(cookie);
			}
		}

		/// <summary>
		/// Copied from Mono library.
		/// </summary>
		/// <see cref="http://anonsvn.mono-project.com/viewvc/trunk/mcs/class/System/System.Net/CookieContainer.cs?revision=140055&amp;view=markup"/>
		private static string ParseAndAddCookies_GetCookieValue(string str, int length, ref int i)
		{
			if (i >= length)
				return null;

			int k = i;
			while (k < length && Char.IsWhiteSpace(str[k]))
				k++;

			int begin = k;
			while (k < length && str[k] != ';')
				k++;

			i = k;
			return str.Substring(begin, i - begin).Trim();
		}

		/// <summary>
		/// Copied from Mono library.
		/// </summary>
		/// <see cref="http://anonsvn.mono-project.com/viewvc/trunk/mcs/class/System/System.Net/CookieContainer.cs?revision=140055&amp;view=markup"/>
		private static string ParseAndAddCookies_GetCookieName(string str, int length, ref int i)
		{
			if (i >= length)
				return null;

			int k = i;
			while (k < length && Char.IsWhiteSpace(str[k]))
				k++;

			int begin = k;
			while (k < length && str[k] != ';' && str[k] != '=')
				k++;

			i = k + 1;
			return str.Substring(begin, k - begin).Trim();
		} 

		/// <summary>
		/// Copied from Mono library.
		/// </summary>
		/// <see cref="http://anonsvn.mono-project.com/viewvc/trunk/mcs/class/System/System.Net/CookieContainer.cs?revision=140055&amp;view=markup"/>
		private static string ParseAndAddCookies_GetDir(string path)
		{
			if (path == null || path == "")
				return "/";

			int last = path.LastIndexOf('/');
			if (last == -1)
				return "/" + path;

			return path.Substring(0, last + 1);
		} 

		/// <summary>
		/// Generates cookie string to sending with http request header
		/// </summary>
		/// <param name="cookie"></param>
		/// <returns></returns>
		private string GetCookieHeader(Cookie cookie)
		{
			return CallCookieToServerString(cookie);
		}

		private static Type _cookieContainerType = Type.GetType("System.Net.CookieContainer, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
		private static Type _pathListType = Type.GetType("System.Net.PathList, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
		/// <summary>
		/// This method is aimed to fix a goddamn CookieContainer issue,
		/// In CookieContainer adds missed path for cookies that is not started with dot.
		/// This is a dirty hack
		/// </summary>
		/// <remarks>
		/// This method is only for .NET 2.0 which is used by .NET 3.0 and 3.5 too.
		/// The issue will be fixed in .NET 4, I hope!
		/// </remarks>
		/// <autor>Many thanks to CallMeLaNN "dot-net-expertise.blogspot.com" to complete this method</autor>
		private void BugFix_AddDotCookieDomain(CookieContainer cookieContainer)
		{
			Hashtable table = (Hashtable)_cookieContainerType.InvokeMember("m_domainTable",
											 System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance,
											 null,
											 cookieContainer,
											 new object[] { });

			ArrayList keys = new ArrayList(table.Keys);

			object pathList1;
			object pathList2;

			SortedList sortedList1;
			SortedList sortedList2;
			ArrayList pathKeys;

			CookieCollection cookieColl1;
			CookieCollection cookieColl2;

			foreach (string key in keys)
			{
				if (key[0] == '.')
				{
					string nonDotKey = key.Remove(0, 1);
					// Dont simply code like this:
					// table[nonDotKey] = table[key];
					// instead code like below:

					// This codes will copy all cookies in dot domain key into nondot domain key.

					pathList1 = table[key];
					pathList2 = table[nonDotKey];
					if (pathList2 == null)
					{
						pathList2 = Activator.CreateInstance(_pathListType); // Same as PathList pathList = new PathList();
						lock (cookieContainer)
						{
							table[nonDotKey] = pathList2;
						}
					}

					// merge the PathList, take cookies from table[keyObj] copy into table[nonDotKey]
					sortedList1 = (SortedList)_pathListType.InvokeMember("m_list", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, pathList1, new object[] { });
					sortedList2 = (SortedList)_pathListType.InvokeMember("m_list", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, pathList2, new object[] { });

					pathKeys = new ArrayList(sortedList1.Keys);

					foreach (string pathKey in pathKeys)
					{

						cookieColl1 = (CookieCollection)sortedList1[pathKey];
						cookieColl2 = (CookieCollection)sortedList2[pathKey];
						if (cookieColl2 == null)
						{
							cookieColl2 = new CookieCollection();
							sortedList2[pathKey] = cookieColl2;
						}

						foreach (Cookie c in cookieColl1)
						{
							lock (cookieColl2)
							{
								cookieColl2.Add(c);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns cookie name to store in user's browser
		/// </summary>
		/// <returns>A cookie name</returns>
		public string GetCookieNameByDomain_FrontEnd(Cookie cookie, Uri url)
		{
			if (cookie == null || url == null)
				return null;

			if (string.IsNullOrEmpty(cookie.Domain))
			{
				return GetCookieNameByHost(url.Host);
			}
			else
			{
				// Cookie domain name is best name to use
				// it can be something like ".domain.com" or "domain.com" , both are ok
				return GetCookieNameByHost(cookie.Domain);
			}
		}

		public override string GetCookieName(Uri uri)
		{
			//Uri uri = new Uri(url);
			return GetCookieNameByHost(uri.Host);
		}

		public override string GetCookieName(string url)
		{
			Uri uri;
			if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
			{
				return GetCookieNameByHost(uri.Host);
			}
			throw new UriFormatException("Specified Url is invalid. Url: " + url);
		}

		/// <summary>
		/// Applies asproxy cookie extension to a cookie name or hostname
		/// </summary>
		private static string GetCookieNameByHost(string host)
		{
			return host + strCookieNameExt;
		}

		private static bool IsASProxyCookie(string name)
		{
			return name.EndsWith(strCookieNameExt);
		}

		private string GetCookieHeader(CookieCollection cookieCollection)
		{
			string result = "";
			if (cookieCollection != null)
			{

				for (int i = 0; i < cookieCollection.Count; i++)
				{
					Cookie cookie = cookieCollection[i];

					// generate cookie
					string cookieStr = CallCookieToServerString(cookie);

					if (!string.IsNullOrEmpty(cookieStr))
					{
						if (!string.IsNullOrEmpty(result))
							result = result + ", " + cookieStr;
						else
							result = cookieStr;
					}

					//if ((cookieStr != null) && (cookieStr.Length != 0))
					//{
					//    result = (result == null) ? cookieStr : (result + ", " + cookieStr);
					//    //if ((cookie.Variant == CookieVariant.Rfc2965) || (this.HttpListenerContext.PromoteCookiesToRfc2965 && (cookie.Variant == CookieVariant.Rfc2109)))
					//    //{
					//    //    str = (str == null) ? cookieStr : (str + ", " + cookieStr);
					//    //}
					//    //else
					//    //{
					//    //    str2 = (str2 == null) ? cookieStr : (str2 + ", " + cookieStr);
					//    //}
					//}
				}
			}

			return result;
		}

		/// <summary>
		/// Cookie has a method named "ToServerString" which returns its cookie header for transferring in the net.
		/// It is hidden!
		/// "ToServerString" is for Microsoft .NET framework
		/// "ToClientString" is for Mono framework
		/// </summary>
		private static MethodInfo _cookieHeaderGeneratorMethod;
		private string CallCookieToServerString(Cookie cookie)
		{
			// Get the method type info
			if (_cookieHeaderGeneratorMethod == null)
			{
				if (IsRunningOnMicrosoftCLR)
					_cookieHeaderGeneratorMethod = typeof(Cookie).GetMethod("ToServerString", BindingFlags.Instance | BindingFlags.NonPublic);
				else
					_cookieHeaderGeneratorMethod = typeof(Cookie).GetMethod("ToClientString", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			return _cookieHeaderGeneratorMethod.Invoke(cookie, null).ToString();
		}


		static Type _cookieType = typeof(Cookie);
		private static void BugFix_ResetCookieDomainImplicit(Cookie cookie)
		{
			//"m_domain_implicit"
			_cookieType.GetField("m_domain_implicit",
									   System.Reflection.BindingFlags.NonPublic |
									   System.Reflection.BindingFlags.GetField |
									   System.Reflection.BindingFlags.Instance)
									   .SetValue(cookie, true);
		}




	}
}