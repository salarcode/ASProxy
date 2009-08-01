using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;

namespace SalarSoft.ASProxy
{
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