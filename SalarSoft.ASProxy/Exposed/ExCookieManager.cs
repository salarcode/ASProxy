using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Collections.Specialized;

namespace SalarSoft.ASProxy.Exposed
{
    public abstract class ExCookieManager : ICookieManager
    {
		public abstract void RestoreCookiesFromResponse(System.Net.WebResponse webResponse, bool saveAsTemporary);
        public abstract void AddCookiesToRequest(System.Net.WebRequest webRequest);
		public abstract void RestoreCookiesFromResponse(System.Net.WebResponse webResponse, System.Net.CookieCollection cookies);
        public abstract void AddCookiesToRequest(System.Net.WebRequest webRequest, System.Net.CookieCollection cookies);
		public abstract string GetCookieName(Uri url);
		public abstract string GetCookieName(string url);
		public abstract StringCollection GetAppliedCookieNamesList(string urlHost);
		public abstract string GetCookieNameExt { get; }

	}
}