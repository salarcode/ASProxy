using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SalarSoft.ASProxy.Exposed
{
	internal partial class PluginMethods
	{
		internal enum IPluginCredentialCache
		{
			AfterAddCertification,
			OnGetNetworkCertification
		}
	}

	public interface IPluginCredentialCache
	{
		/// <summary>
		/// 
		/// </summary>
		void AfterAddCertification(ICredentialCache credentialCache, string url, string userName, string password);

		/// <summary>
		/// 
		/// </summary>
		void OnGetNetworkCertification(ICredentialCache credentialCache, string url, NetworkCredential result);

	}
}
