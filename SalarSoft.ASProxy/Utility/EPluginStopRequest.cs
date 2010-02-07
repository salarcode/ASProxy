using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;

namespace SalarSoft.ASProxy
{
	/// <summary>
	/// A plugin may throw this excpetion if it wants to stop the request
	/// </summary>
	public class EPluginStopRequest : HttpException
	{
		const string _message = "The operation is stopped permentyle by a plugin.";
		public EPluginStopRequest()
			: base((int)HttpStatusCode.InternalServerError, _message)
		{
		}

		public EPluginStopRequest(string message)
			: base((int)HttpStatusCode.InternalServerError, message)
		{
		}

		public EPluginStopRequest(int httpCode, string message)
			: base(httpCode, message)
		{
		}

		public EPluginStopRequest(string message, Exception innerException)
			: base((int)HttpStatusCode.InternalServerError, message, innerException)
		{
		}
	}
}