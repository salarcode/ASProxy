﻿using System;
using System.Data;
using System.Configuration;
using System.Web;

namespace SalarSoft.ASProxy.Exposed
{
    /// <summary>
    /// Summary description for ExCookieManager
    /// </summary>
    public abstract class ExCookieManager : ICookieManager
    {
		public abstract void RestoreCookiesFromResponse(System.Net.WebResponse webResponse, bool saveAsTemporary);
        public abstract void AddCookiesToRequest(System.Net.WebRequest webRequest);
		public abstract void RestoreCookiesFromResponse(System.Net.WebResponse webResponse, System.Net.CookieCollection cookies);
        public abstract void AddCookiesToRequest(System.Net.WebRequest webRequest, System.Net.CookieCollection cookies);
		public abstract string GetCookieName(Uri url);
		public abstract string GetCookieName(string url);
	}
}