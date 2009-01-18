using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace SalarSoft.ASProxy
{
	public class CookieManager
	{
		private const string strCookieHeaderSeperator = "; ";
		private const string strCookieNameExt = "_Cookies";

		private static string GetCookieName(string host)
		{
			return host + strCookieNameExt;
		}

		internal static string GetCookieNameRequest(string pageUrl)
		{
			Uri url = new Uri(pageUrl);
			return GetCookieName(url.Host);
		}


		/// <summary>
		/// Saves response cookies in a cookie collection
		/// </summary>
		/// <param name="httpResponse">proxy response instance</param>
		/// <param name="cookies">Specified cookie collection</param>
		public static void RestoreCookiesFromResponse(WebResponse httpResponse, CookieCollection cookies)
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
		public static void RestoreCookiesFromResponse(WebResponse httpResponse)
		{
			// ---------------------------------------
			// Declarations
			if (!(httpResponse is HttpWebResponse) || HttpContext.Current == null)
				return;

			HttpResponse userResponse = HttpContext.Current.Response;
			if (userResponse == null)
				return;
			HttpRequest userRequest = HttpContext.Current.Request;
			HttpWebResponse webResponse = (HttpWebResponse)httpResponse;
			int defaultCookieTimeout=20;

			if(HttpContext.Current.Session!=null)
				defaultCookieTimeout=HttpContext.Current.Session.Timeout;

			string hostName = webResponse.ResponseUri.Host;
			Uri webUri = webResponse.ResponseUri;

			// ---------------------------------------
			// The code


			// Returned cookies
			Hashtable cookies = new Hashtable();
			for (int i = 0; i < webResponse.Cookies.Count; i++)
			{
				// getting response cookie
				Cookie webCookie = webResponse.Cookies[i];
				string cookieName;

				// Choosing cookie name
				if (string.IsNullOrEmpty(webCookie.Domain) == false)
				{
					cookieName = webCookie.Domain;

					// BUGFIX: First dot is not necessary
					// I don't care what RFC 2109 says, however removing this first dot doesn't break it!
					// It is fine with RFC
					// Since v5.0
					if (cookieName.StartsWith(".www"))
						cookieName = cookieName.Remove(0, 1);
				}
				else
					cookieName = hostName;

				// Applying cookie name
				cookieName = GetCookieName(cookieName);

				// Get cookie from collection if any exists
				CookieCollection cookieColl;
				cookieColl = (CookieCollection)cookies[cookieName];

				// check existance
				if (cookieColl == null)
				{
					// if not create a new one
					cookieColl = new CookieCollection();
					cookies[cookieName] = cookieColl;
				}

				// Adding response cookie to collection
				cookieColl.Add(webCookie);
			}


			DateTime defaultExpireDate = DateTime.Now.AddMinutes(defaultCookieTimeout);

			// Combine proxy cookies with user cookies then add them to user
			foreach (DictionaryEntry entry in cookies)
			{
				// the collection
				CookieCollection cookieColl = (CookieCollection)entry.Value;

				// the cookie name
				string cookieName = (string)entry.Key;
				string cookieText;
				DateTime expireDate;

				// Read user cookie if there is any
				HttpCookie userCookie = userRequest.Cookies[cookieName];

				// Check existence
				if (userCookie == null)
				{
					// if not create a new one
					userCookie = new HttpCookie(cookieName);
					cookieText = Internal.GetCookieCollectionHeader(cookieColl, webUri);
				}
				else
				{
					// if user has the cookie, it is necessary to combine
					// the both cookies
					// Of course, if a value duplicates, the old value should be replcaed with new one
					cookieText = Internal.CombineCookiesAndGetHeaders(cookieColl, userCookie.Value, webUri);
				}


				// Get expiration date
				expireDate = Internal.FindMaximumExpireDate(cookieColl, defaultExpireDate);

				// Decode cookies to url wellknown string
				cookieText = HttpUtility.UrlEncode(cookieText);

				// Set expiration if available
				if (expireDate.Equals(defaultExpireDate) == false)
					userCookie.Expires = expireDate;

				// Set new cookie value
				userCookie.Value = cookieText;

				// Add cookie to user request. The cookie will send to user and will be saved there
				userResponse.Cookies.Add(userCookie);
			}
		}

		/// <summary>
		/// Adds specified cookies to proxy request cookies collection
		/// </summary>
		/// <param name="httpRequest">Proxy request</param>
		/// <param name="cookies">Specified cookies</param>
		public static void AddCookiesToRequest(WebRequest httpRequest, CookieCollection cookies)
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
			webRequest.CookieContainer.Add(webRequest.Address, cookies);
		}

		/// <summary>
		/// Reads cookies from ASProxy cookies and adds them to request to site
		/// </summary>
		/// <param name="httpRequest">Proxy request</param>
		public static void AddCookiesToRequest(WebRequest httpRequest)
		{
			// ---------------------------------------
			// Declarations
			if (!(httpRequest is HttpWebRequest) || HttpContext.Current == null)
				return;

			HttpResponse userResponse = HttpContext.Current.Response;
			HttpRequest userRequest = HttpContext.Current.Request;
			if (userRequest == null || userResponse == null)
				return;
			HttpWebRequest webRequest = (HttpWebRequest)httpRequest;

			string hostName = webRequest.Address.Host;
			Uri webUri = webRequest.Address;

			// ---------------------------------------
			// The code

			// Enable the cookies!
			if (webRequest.CookieContainer == null)
				webRequest.CookieContainer = new CookieContainer();

			// Get cookies to be serached
			// The cookie names that cab be applied according to host name
			List<string> cookiesName = Internal.GetCookiesNameToBeSearched(hostName);

			for (int i = 0; i < cookiesName.Count; i++)
			{
				string cookieName = cookiesName[i];

				// Get cookie from user request
				HttpCookie userCookie = userRequest.Cookies[cookieName];

				// If there is no cookie
				if (userCookie == null) continue;

				// Decode requested cookie value,
				userCookie.Value = HttpUtility.UrlDecode(userCookie.Value);

				// Apply cookie to site request
				Internal.ApplyCookiesToContainerByHeader(webRequest.CookieContainer, webUri, userCookie.Value);
			}
		}

		/// <summary>
		/// Copies a CookieCollection to another
		/// </summary>
		public static void CopyCookiesCollection(CookieCollection cookiesSrc, CookieCollection cookiesDest)
		{
			if (cookiesSrc != null && cookiesDest != null)
				cookiesDest.Add(cookiesSrc);
		}

		/// <summary>
		/// Includes some private functions
		/// </summary>
		private class Internal
		{
			public static List<string> GetCookiesNameToBeSearched(string host)
			{
				List<string> result = new List<string>();
				string temp;

				temp = GetCookieName(host);
				result.Add(temp);

				//if (host.Length > 0 && host[0] != '.')
				//{
				//    temp = GetCookieName('.' + host);
				//    result.Add(temp);
				//}

				// Find first dot
				int index = host.LastIndexOf('.');
				index--;
				while (index > -1)
				{
					index = host.LastIndexOf('.', index);
					if (index != -1)
					{
						temp = host.Substring(index, host.Length - index);
						temp = GetCookieName(temp);
						result.Add(temp);
					}
					index--;
				}

				return result;
			}


			/// <summary>
			/// Join cookies and returns header of them. Cookie1 is important and can override others.
			/// </summary>
			public static string CombineCookiesAndGetHeaders(CookieCollection cookies1, string cookies2, Uri cookiesUri)
			{
				// if one of arguments not provided
				if (string.IsNullOrEmpty(cookies2))
					return GetCookieCollectionHeader(cookies1, cookiesUri);

				// if one of arguments not provided
				if (cookies1 == null || cookies1.Count == 0)
					return cookies2;

				// Temp
				CookieContainer container = new CookieContainer();

				// Setting the size
				container.MaxCookieSize = 1024 * 1024 * 1024;// Large number, 1 GB !!! shouldn't care about size.


				NameValueCollection compinedCookies = new NameValueCollection();


				// Split the cookies
				string[] cookies = cookies2.Split(new string[] { strCookieHeaderSeperator }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string cookie in cookies)
				{
					// Add cookie name only
					compinedCookies[cookie] = "";
				}

				foreach (Cookie cookie in cookies1)
				{
					// Add cookie name only
					compinedCookies[cookie.ToString()] = "";
				}

				for (int i = 0; i < compinedCookies.Count; i++)
				{
					try
					{
						// Add cookie to container
						container.SetCookies(cookiesUri, compinedCookies.GetKey(i));
					}
					catch { }
				}

				// Get the header
				return container.GetCookieHeader(cookiesUri);
			}

			/// <summary>
			/// Returns header of cookies as a string
			/// </summary>
			public static string GetCookieCollectionHeader(CookieCollection collection, Uri cookieUrl)
			{
				CookieContainer container = new CookieContainer();
				container.MaxCookieSize = 1024 * 1024 * 1024;// Large number, 1 GB !!! don't care about size.

				// BUGFIX: CookieCollection doesn't return cookies header for FQDN urls without dot
				// So I'm cheating here and I added an extra subdomain
				// Since v5.0


				string host = cookieUrl.Host;
				for (int i = 0; i < collection.Count; i++)
				{
					collection[i].Domain = host;
				}

				container.Add(cookieUrl, collection);
				string cookieHeader = container.GetCookieHeader(cookieUrl);
				
				if(string.IsNullOrEmpty(cookieHeader))
					return container.GetCookieHeader(new Uri(cookieUrl.Scheme + "://cookies." + cookieUrl.Authority));

				return cookieHeader;

			}

			/// <summary>
			/// Adds cookies as a cookie header to CookieContainer
			/// </summary>
			public static void ApplyCookiesToContainerByHeader(CookieContainer container, Uri uri, string cookieHeader)
			{
				if (string.IsNullOrEmpty(cookieHeader))
					return;

				string[] cookies = cookieHeader.Split(new string[] { strCookieHeaderSeperator }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string cookier in cookies)
				{
					try
					{
						container.SetCookies(uri, cookier);
					}
					catch { }
				}
			}

			internal static DateTime FindMaximumExpireDate(CookieCollection cookieColl, DateTime dateTime)
			{
				DateTime max = dateTime;
				for (int i = 0; i < cookieColl.Count; i++)
				{
					Cookie cookie = cookieColl[i];
					if (cookie.Expires > max)
						max = cookie.Expires;
				}
				return max;
			}
		}

	}
}