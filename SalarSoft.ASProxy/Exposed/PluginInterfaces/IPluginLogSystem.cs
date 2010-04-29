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
using System.Web;

namespace SalarSoft.ASProxy.Exposed
{
	internal partial class PluginMethods
	{
		internal enum IPluginLogSystem
		{
			BeforeLog,
			BeforeLogError
		}
	}

	public interface IPluginLogSystem
	{
		/// <summary>
		/// 
		/// </summary>
		void BeforeLog(ILogSystem logSystem, LogEntity entity, HttpRequest request, string requestedUrl, params object[] optionalData);
		
		/// <summary>
		/// 
		/// </summary>
		void BeforeLog(ILogSystem logSystem, LogEntity entity, params object[] optionalData);
		
		/// <summary>
		/// 
		/// </summary>
		void BeforeLogError(ILogSystem logSystem, Exception ex, HttpRequest request, string message, string requestedUrl, params object[] optionalData);
	}
}