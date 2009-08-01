using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SalarSoft.ASProxy.Exposed
{
	internal partial class PluginMethods
	{
		internal enum IPluginCookieManager
		{
			AfterRestoreCookiesFromResponse,
			AfterAddCookiesToRequest
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IPluginCookieManager
	{
		/// <summary>
		/// 
		/// </summary>
		void AfterRestoreCookiesFromResponse(ICookieManager cookieManager, WebResponse webResponse, CookieCollection cookies);

		/// <summary>
		/// 
		/// </summary>
		void AfterAddCookiesToRequest(ICookieManager cookieManager, WebRequest webRequest, CookieCollection cookies);
	}
}
