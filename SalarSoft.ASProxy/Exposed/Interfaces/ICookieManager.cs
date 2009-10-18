using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Net;
using System.Collections.Specialized;

namespace SalarSoft.ASProxy.Exposed
{
    /// <summary>
    /// Generalize the CookieManager
    /// </summary>
    public interface ICookieManager
    {
		string GetCookieName(Uri url);
		string GetCookieName(string url);

		string GetCookieNameExt { get; }

		/// <summary>
		/// Cookie names list which appllies to specifed url
		/// </summary>
		StringCollection GetAppliedCookieNamesList(string urlHost);

		/// <summary>
        /// Saves response cookies in a cookie collection
        /// </summary>
        void RestoreCookiesFromResponse(WebResponse webResponse, CookieCollection cookies);

        /// <summary>
        /// Reads cookies from response and saves them all in user pc
        /// </summary>
        void RestoreCookiesFromResponse(WebResponse webResponse, bool saveAsTemporary);

        /// <summary>
        /// Adds specified cookies to proxy request cookies collection
        /// </summary>
        void AddCookiesToRequest(WebRequest webRequest, CookieCollection cookies);

        /// <summary>
        /// Reads cookies from ASProxy cookies and adds them to request to site
        /// </summary>
        void AddCookiesToRequest(WebRequest webRequest);
    }
}