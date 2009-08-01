using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using SalarSoft.ASProxy.Exposed;
using System.Diagnostics;

namespace SalarSoft.ASProxy.BuiltIn
{
	public class CookieManager : ExCookieManager
	{
		protected const string strCookieHeaderSeperator = "; ";
		protected const string strCookieNameExt = "_Cookie";
		private bool _isPluginAvailable;

		public CookieManager()
		{
			// getting plugin availablity state
			_isPluginAvailable = Plugins.IsPluginAvailable(PluginHosts.IPluginCookieManager);
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
		public override void RestoreCookiesFromResponse(WebResponse httpResponse, bool saveAsTemporary)
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
			int defaultCookieTimeout;

			if (HttpContext.Current.Session != null)
				defaultCookieTimeout = HttpContext.Current.Session.Timeout;
			else
				defaultCookieTimeout = 20;

			string hostName = webResponse.ResponseUri.Host;
			Uri webUri = webResponse.ResponseUri;

			// ---------------------------------------
			// The code

#if TRACE
			string traceCode = "RestoreCookiesFromResponse-----------\n";
			traceCode += "URL: " + webResponse.ResponseUri.ToString() + "\n\n";
			traceCode += "WebResponse Cookies: " + webResponse.Cookies.Count + "\n";
			for (int i = 0; i < webResponse.Cookies.Count; i++)
			{
				// getting response cookie
				Cookie webCookie = webResponse.Cookies[i];
				traceCode += webCookie.ToString() + "\n";
			}
#endif

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
				cookieName = Internal.GetCookieName(cookieName);

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

			// cookies calculated timeout in minute
			DateTime expireDate;

			if (saveAsTemporary)
			{
				// default expire date
				expireDate = DateTime.Now.AddMinutes(defaultCookieTimeout);
			}
			else
			{
				// Since V5.0: To prevent cookie storage overflow, it should store cookies at most 5 days
				expireDate = DateTime.Now.AddDays(5);
			}

			// Combine proxy cookies with user cookies then add them to user
			foreach (DictionaryEntry entry in cookies)
			{
				// the collection
				CookieCollection cookieColl = (CookieCollection)entry.Value;

				// the cookie name
				string cookieName = (string)entry.Key;
				string cookieText;

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
				//expireDate = Internal.FindMaximumExpireDate(cookieColl, defaultExpireDate);

				// Set expiration if needed
				if (expireDate.Equals(userCookie.Expires) == false)
					userCookie.Expires = expireDate;

				//// Set expiration if available
				//if (expireDate.Equals(defaultExpireDate) == false)
				//    userCookie.Expires = expireDate;

#if TRACE
				traceCode += "-------\n";
				traceCode += "Cookes that are going to  CookiesName=" + userCookie.Name+"\n";
				traceCode += cookieText + "\n";
				Systems.LogSystem.LogError("::::CookieManager::::: Tracelog",webResponse.ResponseUri.ToString(),
					traceCode);
#endif

				// Decode cookies to url wellknown string
				cookieText = HttpUtility.UrlEncode(cookieText);

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
			webRequest.CookieContainer.Add(webRequest.Address, cookies);
		}

		/// <summary>
		/// Reads cookies from ASProxy cookies and adds them to request to site
		/// </summary>
		/// <param name="httpRequest">Proxy request</param>
		public override void AddCookiesToRequest(WebRequest httpRequest)
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
		public void CopyCookiesCollection(CookieCollection cookiesSrc, CookieCollection cookiesDest)
		{
			if (cookiesSrc != null && cookiesDest != null)
				cookiesDest.Add(cookiesSrc);
		}

		public override string GetCookieName(Uri url)
		{
			return Internal.GetCookieName(url.Host);
		}

		public override string GetCookieName(string url)
		{
			Uri uri = new Uri(url);
			return Internal.GetCookieName(uri.Host);
		}



		/// <summary>
		/// Includes some private functions
		/// </summary>
		protected static class Internal
		{
			public static string GetCookieName(string host)
			{
				return host + strCookieNameExt;
			}

			public static string GetCookieNameRequest(string pageUrl)
			{
				Uri url = new Uri(pageUrl);
				return GetCookieName(url.Host);
			}


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
				CookieContainer container;

				// BUGFIX: CookieCollection doesn't return cookies header for FQDN urls without dot
				// So I'm cheating here and I added an extra subdomain
				// Since v5.0

				string host = cookieUrl.Host;
				string cookieHeader;

				try
				{
					// BUG::: Facebook requires this method
					
					for (int i = 0; i < collection.Count; i++)
					{
						// BUG: setting the Domain changes "domain_implicit" to false and that 
						// cuases to "container.Add" fail with "The 'Domain'='localhost' part of the cookie is invalid."
						// Facebook!!!!!
						collection[i].Domain = host;
						collection[i].Value = HttpUtility.UrlEncode(collection[i].Value);
					}

					container = new CookieContainer();
					container.MaxCookieSize = 1024 * 1024 * 1024;// Large number, 1 GB !!! don't care about size.

					// this may fail
					container.Add(cookieUrl, collection);
					cookieHeader = container.GetCookieHeader(cookieUrl);

					if (string.IsNullOrEmpty(cookieHeader))
						return container.GetCookieHeader(new Uri(cookieUrl.Scheme + "://cookies." + cookieUrl.Authority));
				}
				catch (Exception)
				{
					// BUG::: Not all sites work with that method, so try this one

					for (int i = 0; i < collection.Count; i++)
					{
						// BUG: setting the Domain changes "domain_implicit" to false and that 
						// cuases to "container.Add" fail with "The 'Domain'='localhost' part of the cookie is invalid."
						// collection[i].Domain = host;
						BugFix_ResetCookieDomainImplicit(collection[i]);
						collection[i].Value = HttpUtility.UrlEncode(collection[i].Value);
					}

					container = new CookieContainer();
					container.MaxCookieSize = 1024 * 1024 * 1024;// Large number, 1 GB !!! don't care about size.

					// this may fail
					container.Add(cookieUrl, collection);
					cookieHeader = container.GetCookieHeader(cookieUrl);

					if (string.IsNullOrEmpty(cookieHeader))
						return container.GetCookieHeader(new Uri(cookieUrl.Scheme + "://cookies." + cookieUrl.Authority));
				}

				return cookieHeader;

			}


			static Type _CookieType = typeof(Cookie);
			private static void BugFix_ResetCookieDomainImplicit(Cookie cookie)
			{
				//"m_domain_implicit"
				_CookieType.GetField("m_domain_implicit",
										   System.Reflection.BindingFlags.NonPublic |
										   System.Reflection.BindingFlags.GetField |
										   System.Reflection.BindingFlags.Instance)
										   .SetValue(cookie, true);
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