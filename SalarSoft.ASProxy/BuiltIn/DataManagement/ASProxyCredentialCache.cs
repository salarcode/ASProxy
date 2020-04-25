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
using System.Net;
using System.Web;
using System.Web.SessionState;
using SalarSoft.ASProxy.Exposed;

namespace SalarSoft.ASProxy.BuiltIn
{
	public class ASProxyCredentialCache : ExCredentialCache
	{
		class CredentialDetails
		{
			public string UserName;
			public string Password;
		}

		#region local variables
		private bool _isPluginAvailable;
		#endregion

		public ASProxyCredentialCache()
		{
			// getting plugin availablity state
			_isPluginAvailable = Plugins.IsPluginAvailable(PluginHosts.IPluginCredentialCache);
		}

		public override bool IsCertificated(string url)
		{
			if (HttpContext.Current == null || HttpContext.Current.Session == null)
				return false;

			string key = GetCertificatedKey(url);
			return (HttpContext.Current.Session[key] != null);
		}

		public override void AddCertification(string url, string userName, string password)
		{
			HttpSessionState session = HttpContext.Current.Session;
			if (session == null) return;

			string key = GetCertificatedKey(url);
			CredentialDetails details = new CredentialDetails();
			details.UserName = userName;
			details.Password = password;
			session[key] = details;

			// 0- executing plugins
			if (_isPluginAvailable)
				Plugins.CallPluginMethod(PluginHosts.IPluginCredentialCache,
					PluginMethods.IPluginCredentialCache.AfterAddCertification,
					this, url, userName, password);
		}

		public override NetworkCredential GetNetworkCertification(string url)
		{
			NetworkCredential result = GetNetworkCertification(GetCertification(url));
			
			// 1- executing plugins
			if (_isPluginAvailable)
				Plugins.CallPluginMethod(PluginHosts.IPluginCredentialCache,
					PluginMethods.IPluginCredentialCache.OnGetNetworkCertification,
					this, url, result);

			return result;
		}

		private NetworkCredential GetNetworkCertification(CredentialDetails credit)
		{
			return new NetworkCredential(credit.UserName, credit.Password);
		}

		private CredentialDetails GetCertification(string url)
		{
			string key = GetCertificatedKey(url);
			return (CredentialDetails)HttpContext.Current.Session[key];
		}

		private string GetCertificatedKey(string url)
		{
			Uri reqUri = new Uri(url);
			return reqUri.Scheme + reqUri.Port + reqUri.Host;
		}

	}
}