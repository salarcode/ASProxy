using System;
using System.Net;
using System.Web;
using System.Reflection;
using SalarSoft.ASProxy.Exposed;
using System.Collections;

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
			// ---------------------------------------
			// Declarations
			if (!(webResponse is HttpWebResponse) || HttpContext.Current == null)
				return;
			HttpResponse userResponse = HttpContext.Current.Response;
			HttpRequest userRequest = HttpContext.Current.Request;

			HttpWebResponse httpWebResponse = (HttpWebResponse)webResponse;
			Uri webUri = httpWebResponse.ResponseUri;

			// if some cookies is sent
			if (httpWebResponse.Cookies.Count > 0)
			{
				// Get cookie from user request
				string cookieName = GetCookieName(webUri);

				HttpCookie reqCookie = userRequest.Cookies[cookieName];
				CookieContainer container = new CookieContainer();

				// if there is cookies 
				// restores cookies from user request
				ApplyRequestToCookieContainer(container, userRequest);

				CookieCollection responseCookies = httpWebResponse.Cookies;

				// add response cookies
				// this add overrides previous cookies
				//BUFIX: container.Add(webUri, responseCookies);
				container.Add(responseCookies);

				// Only for Micosoft .NET Framework
				if (IsRunningOnMicrosoftCLR)
					// BUGFIX: CookieContainer has a bug, here is its bugfix
					// To get around this bug, the domains should start with a DOT
					BugFix_AddDotCookieDomain(container);

				// get cookie container cookies
				// BUG: CookieContainer has a bug
				// BUG: if the "domain" is ".site.com" or "site.com" it won't return any cookie for "http://site.com"
				CookieCollection coll = container.GetCookies(webUri);

				// get cookie header
				string cookieHeader = GetCookieHeader(coll);

				// if there is cookie header
				if (!string.IsNullOrEmpty(cookieHeader))
				{
					reqCookie = new HttpCookie(cookieName);

					if (!saveAsTemporary)
					{
						// Since V5.0: To prevent cookie storage overflow, it should store cookies at most 7 days
						reqCookie.Expires = DateTime.Now.AddDays(7);
					}

					// value
					reqCookie.Value = HttpUtility.UrlEncode(cookieHeader);

					// add to response
					userResponse.Cookies.Add(reqCookie);
				}
			}
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
		/// Reads request cookie to cookie container
		/// </summary>
		/// <param name="container"></param>
		/// <param name="userRequest"></param>
		private void ApplyRequestToCookieContainer(CookieContainer container, HttpRequest userRequest)
		{
			if (container == null || userRequest == null)
				return;

			// if there is cookies 
			for (int i = 0; i < userRequest.Cookies.Count; i++)
			{
				HttpCookie cookie = userRequest.Cookies[i];
				if (IsASProxyCookie(cookie.Name))
				{
					// BUGFIX: cookies header shouldn't be decoded
					//string val = HttpUtility.UrlDecode(cookie.Value);
					string val = cookie.Value;

					Uri host = GetASProxyCookieHost(cookie.Name);
					if (host != null)
						container.SetCookies(host, val);
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
			ApplyRequestToCookieContainer(httpWebRequest.CookieContainer, userRequest);

			// Only for Micosoft .NET Framework
			if (IsRunningOnMicrosoftCLR)
				// BUGFIX: CookieContainer has a bug
				// Here is its bugfix
				// To get around this bug, the domains should start with a DOT
				BugFix_AddDotCookieDomain(httpWebRequest.CookieContainer);

		}


		public string GetCookieNameByDomain(Uri uri)
		{
			//Uri uri = new Uri(url);
			return GetCookieNameByHost(uri.Host);
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

		private static string GetCookieNameByHost(string host)
		{
			string cookieName = host + strCookieNameExt;
			//if (cookieName.StartsWith(".www"))
			//	cookieName = cookieName.Remove(0, 1);
			return cookieName;
		}

		private static bool IsASProxyCookie(string name)
		{
			return name.EndsWith(strCookieNameExt);
		}

		private static Uri GetASProxyCookieHost(string name)
		{
			int index = name.LastIndexOf(strCookieNameExt);
			if (index != -1)
			{
				string host = name.Substring(0, index);
				try
				{
					return new Uri("http://" + host);
				}
				catch (UriFormatException)
				{
					try
					{
						if (host.StartsWith("."))
							host = host.Remove(0, 1);
						return new Uri("http://" + host);
					}
					catch (Exception)
					{
						throw new UriFormatException("failed to parse: " + name);
					}
				}
			}
			return null;
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
		MethodInfo cookieHeaderGeneratorMethod;
		private string CallCookieToServerString(Cookie cookie)
		{
			// Get the method type info
			if (cookieHeaderGeneratorMethod == null)
			{
				if(IsRunningOnMicrosoftCLR)
					cookieHeaderGeneratorMethod = typeof(Cookie).GetMethod("ToServerString", BindingFlags.Instance | BindingFlags.NonPublic);
				else
					cookieHeaderGeneratorMethod = typeof(Cookie).GetMethod("ToClientString", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			return cookieHeaderGeneratorMethod.Invoke(cookie, null).ToString();
		}

	}
}