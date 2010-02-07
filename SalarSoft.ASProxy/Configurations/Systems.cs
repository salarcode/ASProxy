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
