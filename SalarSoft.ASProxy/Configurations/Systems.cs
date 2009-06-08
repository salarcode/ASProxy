using System;
using System.Collections.Generic;
using System.Text;
using SalarSoft.ASProxy.Exposed;

namespace SalarSoft.ASProxy
{
    /// <summary>
    /// Utility classes instances to use in project
    /// </summary>
    public static class Systems
    {
        #region variables
        private static ICookieManager _exCookieManager;
        private static ILogSystem _exLogSystem;
        private static ICredentialCache _exCredentialCache;
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
        #endregion

        #region static methods
        static Systems()
        {
            _exCookieManager = (ICookieManager)Provider.CreateProviderInstance(ProviderType.ICookieManager);
            _exLogSystem = (ILogSystem)Provider.CreateProviderInstance(ProviderType.ILogSystem);
            _exCredentialCache = (ICredentialCache)Provider.CreateProviderInstance(ProviderType.ICredentialCache);
        }
        #endregion
    }
}
