using System;
using System.Collections.Generic;
using System.Text;
using SalarSoft.ASProxy.Exposed;

namespace SalarSoft.ASProxy
{
    /// <summary>
	/// Instances of utility classes to use in everywhere. These classes are shared between requests.
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
        public static ICookieManager CookieManager
        {
            get { return Systems._exCookieManager; }
        }
        public static ILogSystem LogSystem
        {
            get { return Systems._exLogSystem; }
        }
		public static ICredentialCache CredentialCache
		{
			get { return Systems._exCredentialCache; }
		}
		public static IUAC UAC
		{
			get { return Systems._exUAC; }
		}
		#endregion

        #region static methods
        static Systems()
        {
            _exCookieManager = (ICookieManager)Provider.GetProvider(ProviderType.ICookieManager);
            _exLogSystem = (ILogSystem)Provider.GetProvider(ProviderType.ILogSystem);
			_exCredentialCache = (ICredentialCache)Provider.GetProvider(ProviderType.ICredentialCache);
			_exUAC = (IUAC)Provider.GetProvider(ProviderType.IUAC);
		}
        #endregion
    }
}
