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
