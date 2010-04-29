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
// The Initial Developer of the Original Code is Salar.Kh (SalarSoft).
// Portions created by Salar.Kh are Copyright (C) 2010 All Rights Reserved.
// 
// Contributor(s): 
// Salar.Kh < salarsoftwares [@] gmail[.]com > (original author)
//
//**************************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace SalarSoft.ASProxy
{
	public enum PluginHosts
	{
		/// <summary>
		/// All the units works for ASProxy's Engine. This class uses other units to
		/// analyze the request, send and recevicing data, processing data and sending
		/// back to the front end user.
		/// </summary>
		IPluginEngine,

		/// <summary>
		/// Works with backend server or website which frontend user wanted to see.
		/// Any request uses this tool to get data from web.
		/// </summary>
		IPluginWebData,

		/// <summary>
		/// Saves and restores cookies for a request.
		/// </summary>
		IPluginCookieManager,

		/// <summary>
		/// Saves authenticated credentials in a safe place for further uses.
		/// </summary>
		IPluginCredentialCache,

		/// <summary>
		/// Analyzes and processes the CSS (Cascade style sheet) data.
		/// </summary>
		IPluginCSSProcessor,

		/// <summary>
		/// Analyzes and processes the Html data.
		/// </summary>
		IPluginHtmlProcessor,

		/// <summary>
		/// Analyzes and processes the JavaScript data.
		/// </summary>
		IPluginJSProcessor,

		/// <summary>
		/// Used to system errors and user activies to a desired place.
		/// </summary>
		IPluginLogSystem,

		/// <summary>
		/// Used to block user with specified IPs from using this service.
		/// </summary>
		IPluginUAC

	}
}
