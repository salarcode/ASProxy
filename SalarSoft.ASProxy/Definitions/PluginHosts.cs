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
