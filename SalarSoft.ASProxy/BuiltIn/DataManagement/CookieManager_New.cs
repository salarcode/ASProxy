using System;
using System.Net;
using System.Web;
using System.Reflection;
using SalarSoft.ASProxy.Exposed;
using System.Collections;

namespace SalarSoft.ASProxy.BuiltIn
{
	public class CookieManager : ExCookieManager
	{
		protected const string strCookieNameExt = "_ASPX";


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
				container.Add(webUri, responseCookies);

				// BUGFIX: CookieContainer has a bug
				// Here is its bugfix
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

		/// <summary>
		/// CookieContainer "Domain" BugFix
		/// </summary>
		/// <param name="responseCookies"></param>
		private void BugFix_AddDotCookieDomain(CookieCollection responseCookies)
		{
			// #warning cookie dirty modifiying
			foreach (Cookie cookie in responseCookies)
			{
				string domain = cookie.Domain;
				if (!string.IsNullOrEmpty(domain))
				{
					if (domain[0] != '.')
						domain = '.' + domain;
					//if (domain[0] == '.')
					//    domain = domain.Remove(0, 1);
					cookie.Domain = domain;
				}
			}
		}

		Type _ContainerType = typeof(CookieContainer);

		/// <summary>
		/// CookieContainer "Domain" BugFix
		/// </summary>
		/// <param name="responseCookies"></param>
		private void BugFix_AddDotCookieDomain(CookieContainer cookieContainer)
		{
			// here is the hack: http://social.microsoft.com/Forums/en-US/netfxnetcom/thread/1297afc1-12d4-4d75-8d3f-7563222d234c
			// and: http://channel9.msdn.com/forums/TechOff/260235-Bug-in-CookieContainer-where-do-I-report/
			Hashtable table = (Hashtable)_ContainerType.InvokeMember("m_domainTable",
									   System.Reflection.BindingFlags.NonPublic |
									   System.Reflection.BindingFlags.GetField |
									   System.Reflection.BindingFlags.Instance,
									   null,
									   cookieContainer,
									   new object[] { });
			ArrayList keys = new ArrayList(table.Keys);
			foreach (string keyObj in keys)
			{
				string key = (keyObj as string);
				if (key[0] == '.')
				{
					string newKey = key.Remove(0, 1);
					table[newKey] = table[keyObj];
				}
			}
		}


		public void ApplyRequestToCookieContainer(CookieContainer container, HttpRequest userRequest)
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
			webRequest.CookieContainer.Add(webRequest.Address, cookies);
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

			if (cookieName.StartsWith(".www"))
				cookieName = cookieName.Remove(0, 1);
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

		MethodInfo methodCookieToServerString = typeof(Cookie).GetMethod("ToServerString", BindingFlags.Instance | BindingFlags.NonPublic);
		private string CallCookieToServerString(Cookie cookie)
		{
			return methodCookieToServerString.Invoke(cookie, null).ToString();
		}

	}
}