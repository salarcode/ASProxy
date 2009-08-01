using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace SalarSoft.ASProxy.Exposed
{
	internal partial class PluginMethods
	{
		internal enum IPluginEngine
		{
			AfterInitialize,
			BeforeHandshake,
			AfterHandshake,
			BeforeProcessor,
			AfterProcessor,
			AfterExecuteToResponse
		}
	}

	public interface IPluginEngine
	{

		/// <summary>
		/// 0-
		/// The front-end request is given and processed.
		/// Here plugin can modify the request parameters.
		/// </summary>
		void AfterInitialize(IEngine engine);

		/// <summary>
		/// 1-
		/// Before sending the request to back-end website.
		/// Plugin can modify request here.
		/// </summary>
		void BeforeHandshake(IEngine engine, IWebData webData);

		/// <summary>
		/// 2-
		/// The handshake is done here and data is available.
		/// Plugin can modify response data and headers.
		/// </summary>
		void AfterHandshake(IEngine engine, IWebData webData);

		/// <summary>
		/// 3-
		/// The response data need some proccess.
		/// Plugin can modify the processor.
		/// </summary>
		void BeforeProcessor(IEngine engine, IDataProcessor dataProcessor);

		/// <summary>
		/// 4-
		/// The processing is done.
		/// Plugin can modify the results in the Engine
		/// </summary>
		void AfterProcessor(IEngine engine);

		/// <summary>
		/// 5-
		/// Everything is done and the data is ready to send to front-end user.
		/// This method will be called only by some kind of data and data types.
		/// Plugin can modify the response.
		/// </summary>
		void AfterExecuteToResponse(IEngine engine, HttpResponse httpResponse);
	}
}
