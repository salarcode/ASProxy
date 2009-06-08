using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Net;

namespace SalarSoft.ASProxy.Exposed
{
    /// <summary>
    /// Generalize the CookieManager
    /// </summary>
    public interface ICookieManager
    {
        string GetCookieName(string url);
        /// <summary>
        /// Saves response cookies in a cookie collection
        /// </summary>
        void RestoreCookiesFromResponse(WebResponse webResponse, CookieCollection cookies);

        /// <summary>
        /// Reads cookies from response and saves them all in user pc
        /// </summary>
        void RestoreCookiesFromResponse(WebResponse webResponse);

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