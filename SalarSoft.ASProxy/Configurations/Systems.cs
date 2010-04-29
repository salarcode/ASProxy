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
using SalarSoft.ASProxy.Exposed;

namespace SalarSoft.ASProxy
{
    /// <summary>
	/// Instances of utility classes to use in everywhere.
	/// These classes are shared in application to be used between requests.
    /// </summary>
    public static class Systems
    {
        #region variables
        private static ICookieManager _exCookieManager;
        private static ILogSystem _exLogSystem;
		private static ICredentialCache _exCredentialCache;
		private static IUAC _exUAC;
		#endregion

        #region properties
		/// <summary>
		/// General cookie manager
		/// </summary>
        public static ICookieManager CookieManager
        {
            get { return Systems._exCookieManager; }
        }

		/// <summary>
		/// General LogSystem
		/// </summary>
        public static ILogSystem LogSystem
        {
            get { return Systems._exLogSystem; }
        }

		/// <summary>
		/// General credential cache system
		/// </summary>
		public static ICredentialCache CredentialCache
		{
			get { return Systems._exCredentialCache; }
		}

		/// <summary>
		/// General UAC
		/// </summary>
		public static IUAC UAC
		{
			get { return Systems._exUAC; }
		}
		#endregion

        #region static methods
        static Systems()
        {
            _exCookieManager = (ICookieManager)Providers.GetProvider(ProviderType.ICookieManager);
            _exLogSystem = (ILogSystem)Providers.GetProvider(ProviderType.ILogSystem);
			_exCredentialCache = (ICredentialCache)Providers.GetProvider(ProviderType.ICredentialCache);
			_exUAC = (IUAC)Providers.GetProvider(ProviderType.IUAC);
		}
        #endregion
    }
}
