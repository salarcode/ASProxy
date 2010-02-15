using System;
using System.Net;
using System.Web;
using System.Reflection;
using SalarSoft.ASProxy.Exposed;
using System.Collections;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace SalarSoft.ASProxy.BuiltIn
{
	/// <summary>
	/// Oh god, I could find the problem of cookies and it was damn microsoft's CookieContainer bug!
	/// I've reported this fucking bug, and it is mysteriously fixed in a day! 
	/// It is late now and 3 releases of .net have this bug, what should I do? Damn it! Fixed!
	/// 
	/// This class can work with bugless CookieContainer which arrives with .NET 4.0 .
	/// </summary>
	/// <autors>
	/// Originally developed my SalarSoft
	/// CookieContainer bugfix and contributor CallMeLaNN
	/// </autors>
	public class CookieManager : ExCookieManager
	{
		protected const string strCookieDateTimeFormat = "yyyy/dd/MMM  HH:mm:ss";
		/// <summary>
		/// 6 days
		/// </summary>
		protected const int intExpireDateNormalDays = 6;
		/// <summary>
		/// 30 minutes
		/// </summary>
		protected const int intExpireDateTempMinutes = 30;
		protected const string strCookieNameExt = "_ASPX";
		protected static readonly bool IsRunningOnMicrosoftCLR;
		protected static readonly bool IsRunningOnDotNet4;

		static CookieManager()
		{
			IsRunningOnMicrosoftCLR = !Common.IsRunningOnMono();
			IsRunningOnDotNet4 = Common.IsRunningOnDotNet4();
		}

		public override string GetCookieNameExt { get { return strCookieNameExt; } }


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

				// BUGFIX: this is an temporary solution for when cookies are not specifed by back-end website
				// And HttpWebResponse didn't recognize them well, so it sets extra Path value, which is not correct
				bool isPathSpecified = false;
				string setCookieHeader = httpWebResponse.Headers[HttpResponseHeader.SetCookie];
				if (!string.IsNullOrEmpty(setCookieHeader) && setCookieHeader.ToLower().IndexOf("path=/") != -1)
				{
					isPathSpecified = true;
				}

				// if there is cookies 
				// restores cookies from user request
				ApplyRequestCookiesToCookieContainer(container, userRequest, responseUrl);

				// Response cookies
				CookieCollection responseCookies = httpWebResponse.Cookies;

				// BUGFIX: httpWebResponse does not parses the cookies in correct format
				// Trying to parse it by Mono library functions
				//responseCookies = ParseResponseCookies(httpWebResponse, responseUrl);
				//---------------

				// Adding response cookies,
				// The new cookies will overwite the previous ones
				// BUG: CookieContainer destoyes Expires value so the cookie never expires!
				// BUFIX: bufix is below
				// container.Add(responseCookies);

				//-------------------------------
				CookieCollection expiredCookies = new CookieCollection();
				// BUFIX: We have to remove expired cookies!
				// Because of CookieContainer bug, this method should do this for us!
				// Damn this CookieContainer
				foreach (Cookie cookie in responseCookies)
				{
					// no expired cookies
					if (cookie.Expired)
					{
						expiredCookies.Add(cookie);
						continue;
					}

					// good!
					container.Add(cookie);
				}


				// Only for Micosoft .NET Framework
				// Bug is fixed in .NET 4.0
				if (IsRunningOnMicrosoftCLR && !IsRunningOnDotNet4)
					// BUGFIX: CookieContainer has a bug, here is its bugfix
					// To get around this bug, the domains should start with a DOT
					BugFix_CookieContaierFix(container);


				// CallMeLaNN:
				// Get cookie header
				// CookieCollection coll = container.GetCookies(webUri);
				// Can't get all cookie header logically because it require Uri that can retrieve cookies on that domain and path only.
				// Cookies in deeper subdomain or deeper path will not taken.
				// We never know what subdomains or paths available here.
				// So in order to get the all cookie in the entire domain, reflection is used.
				// Or else we no need to use container earlier to do this.
				// Container is design for easy to us to get the suitable cookies in the current domain and path + expiracy management.
				// However Microsoft should add .GetAllCookies() method in case of this issue.
				CookieCollection allCookies = GetAllCookies(container);

				// SalarSoft:
				// Ok, we have all cookies, we should not just get them all,
				// Most of them applies to different domain
				// So here i'm going to group them, then we can save them in their own domain

				// Cookies are grouped by thier domains
				Dictionary<string, CookieCollection> cookiesGroup = new Dictionary<string, CookieCollection>();

				// applying cookies in groups
				foreach (Cookie cookie in allCookies)
				{
					// Checking cookie expire option
					// The expired cookie shouldn't be saved
					// BUG: CookieContainer has destoyed Expires value so the cookie never expires!
					if (cookie.Expired)
						continue;

					// Search within expired cookies to locate the original cookie
					foreach (Cookie expired in expiredCookies)
					{
						// note: we do not check for value, because it can be differet
						// but the import parts are tested
						if (expired.Name == cookie.Name &&
							expired.Domain == cookie.Domain &&
							expired.Path == cookie.Path &&
							expired.Port == cookie.Port)
						{
							// just modify expire date to be deleted next time
							cookie.Expires = expired.Expires;
						}
					}

					// BUGFIX: read the comments above
					if (!isPathSpecified)
						cookie.Path = "/";


					// Get cookie name for current
					string cookieName = GetCookieNameByDomain(cookie, responseUrl);

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
					// new cookie by cookie name
					HttpCookie frontendCookie = new HttpCookie(entry.Key);

					// Header
					string cookiesHeader = GetASProxyCookieHeader(entry.Value);

					// Encode cookie header to make it safe
					// CallMeLaNN: This is second layer encode
					cookiesHeader = HttpUtility.UrlEncode(cookiesHeader);

					// expire
					if (!saveAsTemporary)
						frontendCookie.Expires = expireDate;
					else
					{
						// nothing, cookie should expire after the session (when user closed the browser)
					}

					// SalarSoft:
					// Expire the cookie if there is no value to store
					if (string.IsNullOrEmpty(cookiesHeader))
						frontendCookie.Expires = DateTime.Now.AddYears(-5);

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


			// Only for Micosoft .NET Framework
			// Bug is fixed in .NET 4.0
			if (IsRunningOnMicrosoftCLR && !IsRunningOnDotNet4)
				// BUGFIX: CookieContainer has a bug
				BugFix_CookieContaierFix(webRequest.CookieContainer);
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
			Uri requestUrl = httpWebRequest.Address;

			// ---------------------------------------
			// The code

			// Enable the cookies!
			if (httpWebRequest.CookieContainer == null)
				httpWebRequest.CookieContainer = new CookieContainer();


			// restore cookies from user request
			ApplyRequestCookiesToCookieContainer(httpWebRequest.CookieContainer, userRequest, requestUrl);

			// Only for Micosoft .NET Framework
			// Bug is fixed in .NET 4.0
			if (IsRunningOnMicrosoftCLR && !IsRunningOnDotNet4)
				// BUGFIX: CookieContainer has a bug
				// Here is its bugfix
				// To get around this bug, the domains should start with a DOT
				BugFix_CookieContaierFix(httpWebRequest.CookieContainer);
		}

		/// <summary>
		/// Generates cookie name to which will be stored in back-end user's browser
		/// </summary>
		/// <returns>Cookie name with ASProxy suffix</returns>
		public override string GetCookieName(Uri uri)
		{
			return GetCookieNameByHost(uri.Host);
		}

		/// <summary>
		/// Generates cookie name to which will be stored in back-end user's browser
		/// </summary>
		/// <returns>Cookie name with ASProxy suffix</returns>
		public override string GetCookieName(string url)
		{
			Uri uri;
			if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
			{
				return GetCookieNameByHost(uri.Host);
			}
			throw new UriFormatException(String.Format("Specified Url is invalid. Url: {0}", url));
		}

		/// <summary>
		/// Cookie names list which appllies to specifed url
		/// </summary>
		public override StringCollection GetAppliedCookieNamesList(string urlHost)
		{
			return GetCookieNamesListForDomain(urlHost);
		}


		/// <summary>
		/// Reads request cookie and applies them to cookie container
		/// </summary>
		/// <remarks>
		/// Here actually I am not using cookie header format, but I use my own key=value pair.
		/// You can change to make it compatible like cookie header like
		///   cookiename=cookievalue instead of Name=cookiename; Value=cookievalue (I seperate name and value for faster coding and good parsing)
		///   expires=... instead of Expires=...
		/// but actually not necessary to be same like cookie header format because this will never use at http header, but just only stored in 'browser group cookie' and only used by this CookieManager.
		/// This Fix will create Cookie by setting all required property value.
		/// This will store Expires, HttpOnly etc that will managed by CookieContainer.
		/// string -> cookie object
		/// Note: I am not sure if this work with your modified RestoreCookiesFromResponse()
		/// </remarks>
		/// <autor>CallMeLaNN</autor>
		private void ApplyRequestCookiesToCookieContainer(CookieContainer container, HttpRequest userRequest, Uri webUri)
		{
			if (container == null || userRequest == null || webUri == null)
				return;

			// Cookie names list which applies to current url
			StringCollection cookieNamesList = GetCookieNamesListForDomain(webUri.ToString());

			// So we have fetch the cookies by specifed names in list
			for (int i = 0; i < cookieNamesList.Count; i++)
			{

				// Get cookie name for specified Url
				string cookieName = cookieNamesList[i];

				// Read the stored cookies for that Url
				HttpCookie reqCookie = userRequest.Cookies[cookieName];

				// Check if there is cookie
				if (reqCookie == null)
					continue;

				// If cookie value in encoded version, decode it first.
				// This is second layer decode. (optional but required if encoded)
				string header = HttpUtility.UrlDecode(reqCookie.Value);

				// if it is empty
				if (string.IsNullOrEmpty(header.Trim()))
					continue;

				// Use standard & as seperator in 'cookie value' instead of , because Expires can contain comma for GMT date time format and split by , will doing the wrong split.
				string[] cookies = header.Trim().Split('&');
				foreach (string cookie in cookies)
				{
					// not empty headers
					if (string.IsNullOrEmpty(cookie.Trim()))
						continue;

					// New cookie
					Cookie cookieObj;

					// parse the header
					cookieObj = ParseASProxyCookieHeader(cookie, webUri);

					// if is parsed, add to the container
					if (cookieObj != null)
						// Add generated cookie to the container
						container.Add(cookieObj);
				}

				// End of cookie names list
			}

			// Only for Micosoft .NET Framework
			// Bug is fixed in .NET 4.0
			if (IsRunningOnMicrosoftCLR && !IsRunningOnDotNet4)
				// BUGFIX: CookieContainer has a bug, here is its bugfix
				// To get around this bug, the domains should start with a DOT
				BugFix_CookieContaierFix(container);
		}


		/// <summary>
		/// Parses ASProxy cookie header to a cookie object
		/// </summary>
		private Cookie ParseASProxyCookieHeader(string cookieHeader, Uri webUri)
		{
			// New cookie
			Cookie cookieObj;

			// cookie properties seperated by ;
			string[] cookieProperties = cookieHeader.Trim().Split(';');

			// a new cookie
			cookieObj = new Cookie();

			foreach (string cookieProperty in cookieProperties)
			{
				string name, value;
				string prop = cookieProperty.Trim();

				if (string.IsNullOrEmpty(prop))
					continue;

				// Can't use split by equal sign method since 'cookie value' can contain equal sign (like in google, PREF='ID=...') and break this parsing,
				// instead, find the first equal sign.
				// cookieKVP = prop.Split('=');
				int equIndex = prop.IndexOf('=');
				name = prop.Substring(0, equIndex).Trim();
				value = prop.Substring(equIndex + 1, prop.Length - equIndex - 1).Trim();

				// Note that this long property name (Name, Value, Expires, Domain, etc)
				// can be do in short form (N, V, E, D, etc) to minimize cookie size.
				switch (name)
				{
					case "Name":
						// the name can not be empty
						if (string.IsNullOrEmpty(value))
							continue;

						cookieObj.Name = value;
						break;
					case "Value":

						// Second layer decode
						cookieObj.Value = HttpUtility.UrlDecode(value);
						break;
					case "Expires":

						// Note: Javascript returns GMT or UTC datetimes which DateTime class can't parse
						DateTime expires;
						string[] dateTimeFormats = new string[]{
									strCookieDateTimeFormat,
									"ddd, d MMM yyyy hh:mm:ss GMT",
									"ddd, d MMM yyyy hh:mm:ss UTC"};

						if (DateTime.TryParseExact(value, dateTimeFormats, null, DateTimeStyles.None, out expires))
						{
							cookieObj.Expires = expires;
						}
						else
						{
							// No chance, do nothing
							// the cookie will expire after current session
						}
						break;
					case "Domain":
						cookieObj.Domain = value;
						break;
					case "Path":
						cookieObj.Path = value;
						break;
					case "HttpOnly":
						cookieObj.HttpOnly = bool.Parse(value);
						break;
					case "Expired":
						cookieObj.Expired = bool.Parse(value);
						break;
					case "Secure":
						cookieObj.Secure = bool.Parse(value);
						break;
					case "Port":
						// noted that I am not sure about port number, not tested yet to filter it.
						cookieObj.Port = value;
						break;
					case "Version":
						cookieObj.Version = int.Parse(value);
						break;
					case "Discard":
						cookieObj.Discard = bool.Parse(value);
						break;
					case "Comment":
						cookieObj.Comment = value;
						break;
					case "CommentUri":
						cookieObj.CommentUri = new Uri(value);
						break;
				}
			}

			// the name can not be empty
			if (string.IsNullOrEmpty(cookieObj.Name))
				return null;

			// We do not accept expired cookies
			if (cookieObj.Expired)
				return null;

			// SalarSoft:
			// Validating cookie domain name, it should not be empty
			if (string.IsNullOrEmpty(cookieObj.Domain))
				cookieObj.Domain = webUri.Host;

			return cookieObj;
		}


		/// <summary>
		/// Returns all cookies stored in CookieContainer
		/// </summary>
		/// <returns>Collection of all cookies</returns>
		private CookieCollection GetAllCookies(CookieContainer cc)
		{
			if (IsRunningOnMicrosoftCLR)
			{
				CookieCollection lstCookies = new CookieCollection();
				Hashtable table = (Hashtable)_cookieContainerType.InvokeMember("m_domainTable",
												 BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
												 null,
												 cc,
												 new object[] { });
				foreach (object pathList in table.Values)
				{
					SortedList lstCookieCol = (SortedList)_pathListType.InvokeMember("m_list",
															  BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
															  null,
															  pathList,
															  new object[] { });
					foreach (CookieCollection colCookies in lstCookieCol.Values)
						foreach (Cookie c in colCookies)
						{
							lstCookies.Add(c);
						}
				}
				return lstCookies;
			}
			else
			{
				// For Mono
				// the cookies list is stored in a private CookieCollection variable which is called "cookies"

				CookieCollection lstCookies = (CookieCollection)_cookieContainerType.InvokeMember("cookies",
												 BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
												 null,
												 cc,
												 new object[] { });
				return lstCookies;
			}
		}

		/// <summary>
		/// encode/serialize (group all cookies) by using key value pair (actually not exactly same like cookie header)
		/// </summary>
		/// <returns>cookie object -> string</returns>
		/// <autor>CallMeLaNN</autor>
		private string GetASProxyCookieHeader(CookieCollection cookieCollection)
		{
			string result = "";
			if (cookieCollection != null)
			{
				string cookieStr = null;
				for (int i = 0; i < cookieCollection.Count; i++)
				{
					Cookie cookie = cookieCollection[i];

					// Generate cookie

					// The If condition below only write/included if different than default value to minimize cookie size.
					// Note that this long property name can be do in short form and remove space to minimize cookie size.


					// SalarSoft:
					// We shouldn't store expired cookies
					if (cookie.Expired)
						continue;

					// Required values, I think:
					cookieStr = "Name=" + cookie.Name;

					// this actual 'cookie value' should be encoded to avoid ,&% 
					// and other special char used in the 'cookie value' that will break a seperator and parsing later.
					// This is first layer encode. Compulsory.
					if (!string.IsNullOrEmpty(cookie.Value))
						cookieStr += "; Value=" + HttpUtility.UrlEncode(cookie.Value);

					if (cookie.Expires != DateTime.MinValue)
					{
						// Cookie expire date should be in special format
						cookieStr += "; Expires=" +
							cookie.Expires.ToString(strCookieDateTimeFormat);
					}

					if (!string.IsNullOrEmpty(cookie.Domain))
						cookieStr += "; Domain=" + cookie.Domain;

					// Most of time the path is "/" which is a default value, so we don't need to store it
					if (!string.IsNullOrEmpty(cookie.Path) && cookie.Path != "/")
						cookieStr += "; Path=" + cookie.Path;

					if (cookie.HttpOnly)
						cookieStr += "; HttpOnly=" + cookie.HttpOnly.ToString();

					// No need to store expired cookies
					// if (cookie.Expired)
					// 	cookieStr += "; Expired=" + cookie.Expired.ToString();

					if (cookie.Secure)
						cookieStr += "; Secure=" + cookie.Secure.ToString();

					// Additional and rarely used or I don't know, just add if any values:
					if (!string.IsNullOrEmpty(cookie.Port))
						cookieStr += "; Port=" + cookie.Port;

					if (cookie.Version != 0)
						cookieStr += "; Version=" + cookie.Version.ToString();

					if (cookie.Discard)
						cookieStr += "; Discard=" + cookie.Discard.ToString();

					if (!string.IsNullOrEmpty(cookie.Comment))
						cookieStr += "; Comment=" + HttpUtility.UrlEncode(cookie.Comment);

					if (cookie.CommentUri != null)
						cookieStr += "; CommentUri=" + HttpUtility.UrlEncode(cookie.CommentUri.AbsoluteUri);

					if (!string.IsNullOrEmpty(result))
						// Use standard & as seperator in cookie value instead of , because Expires can contain comma for GMT date time format.
						result = result + "& " + cookieStr;
					else
						result = cookieStr;
				}
			}

			return result;
		}

		/// <summary>
		/// Returns cookie names applies to specified Domain
		/// </summary>
		/// <returns>List of ASProxy cookie names</returns>
		private StringCollection GetCookieNamesListForDomain(string urlHost)
		{
			StringCollection result = new StringCollection();
			string cookieName;

			Uri host = new Uri(urlHost);

			// The name of host
			string hostName = host.Host;

			// For local and debug puposes
			if (hostName == "localhost")
			{
				// Cookies that applies only for this domain
				cookieName = GetCookieName(host);
				result.Add(cookieName);

				// This domain cookie for its subdomains
				cookieName = GetCookieNameByHost("." + hostName);
				result.Add(cookieName);
			}

			// Find first dot
			int index = hostName.LastIndexOf('.');

			// check if url has a dot
			if (index != -1)
			{
				// Cookies that applies only for this domain
				cookieName = GetCookieName(host);
				result.Add(cookieName);

				// This domain cookie for its subdomains
				cookieName = GetCookieNameByHost("." + hostName);
				result.Add(cookieName);
			}

			// ignoring firt domain which all of the time is something like .com
			index--;

			while (index > -1)
			{
				index = hostName.LastIndexOf('.', index);
				if (index != -1)
				{
					// Get the name
					cookieName = hostName.Substring(index, hostName.Length - index);

					// cookie name for the spefied host
					cookieName = GetCookieNameByHost(cookieName);

					// Add to the results
					result.Add(cookieName);
				}
				index--;
			}

			return result;
		}

		private static Type _cookieContainerType = Type.GetType("System.Net.CookieContainer, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
		private static Type _pathListType = Type.GetType("System.Net.PathList, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
		/// <summary>
		/// This method is aimed to fix a goddamn CookieContainer issue,
		/// It adds missed path for cookies which are not started with dot.
		/// This is a dirty hack
		/// </summary>
		/// <remarks>
		/// This method is only for .NET 2.0 which is used by .NET 3.0 and 3.5 too.
		/// The issue will be fixed in .NET 4, I hope!
		/// </remarks>
		/// <autor>Many thanks to CallMeLaNN "dot-net-expertise.blogspot.com" to complete this method</autor>
		private void BugFix_CookieContaierFix(CookieContainer cookieContainer)
		{
			Hashtable table = (Hashtable)_cookieContainerType.InvokeMember("m_domainTable",
											 BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
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

					// Don't simply code like this:
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
					sortedList1 = (SortedList)_pathListType.InvokeMember("m_list", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance, null, pathList1, new object[] { });
					sortedList2 = (SortedList)_pathListType.InvokeMember("m_list", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance, null, pathList2, new object[] { });

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
		/// Applies asproxy cookie extension to a cookie name or hostname
		/// </summary>
		private static string GetCookieNameByHost(string host)
		{
			return host + strCookieNameExt;
		}

		/// <summary>
		/// Returns cookie name to store in user's browser
		/// </summary>
		/// <returns>A cookie name</returns>
		private string GetCookieNameByDomain(Cookie cookie, Uri url)
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

		/// <summary>
		/// Checks cookie name if it is ASProxy stored cookie
		/// </summary>
		private static bool IsASProxyCookie(string name)
		{
			return name.EndsWith(strCookieNameExt);
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

	}
}