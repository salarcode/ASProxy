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
using System.Net;

namespace SalarSoft.ASProxy.Exposed
{
	internal partial class PluginMethods
	{
		internal enum IPluginWebData
		{
			BeforeExecuteGetResponse,
			AfterExecuteGetResponse,
			AfterExecuteFinalizeWebResponse,
			AfterExecuteReadResponseData
		}
	}

	public interface IPluginWebData
	{
		/// <summary>
		/// 0-
		/// Everything is ready to send the request to the back-wnd website.
		/// Plugin can modify the request.
		/// </summary>
		void BeforeExecuteGetResponse(IWebData webData, WebRequest webRequest);
		
		/// <summary>
		/// 1-
		/// The request is done, and the response is ready to get.
		/// Plugin can modify the response before every other process.
		/// </summary>
		void AfterExecuteGetResponse(IWebData webData, WebResponse webResponse);

		/// <summary>
		/// 2-
		/// All the headers of request is processed. The data is not readed yet.
		/// Plugin can modify the WebData response results.
		/// </summary>
		void AfterExecuteFinalizeWebResponse(IWebData webData, WebResponse webResponse);
		
		/// <summary>
		/// 3-
		/// Data is read and ready. Everything about WebData is done.
		/// Plugin can apply its final processes.
		/// </summary>
		void AfterExecuteReadResponseData(IWebData webData);
	}
}
