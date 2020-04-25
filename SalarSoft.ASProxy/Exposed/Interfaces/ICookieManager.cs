//**************************************************************************
// Product Name: ASProxy
// Description:  SalarSoft ASProxy is a free and open-source web proxy
// which allows the user to surf the net anonymously.
//                     
// MPL 1.1 License agreement.
// The contents of this file are subject to the Mozilla Public License
// Version 1.1 (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
// http://www.mozilla.org/MPL/
// 
// Software distributed under the License is distributed on an "AS IS"
// basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
// License for the specific language governing rights and limitations
// under the License.
// 
// The Original Code is ASProxy (an ASP.NET Proxy).
// 
// The Initial Developer of the Original Code is Salar.K.
// Portions created by Salar.Kh are Copyright (C) 2010 All Rights Reserved.
// 
// Contributor(s): 
// Salar.K https://github.com/salarcode (original author)
//
//**************************************************************************

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