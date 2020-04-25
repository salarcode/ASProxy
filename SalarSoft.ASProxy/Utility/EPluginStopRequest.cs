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