using System;
using System.Web;
using SalarSoft.ASProxy.Exposed;
using System.Net;

namespace SalarSoft.ASProxy.Exposed
{
    public abstract class ExCredentialCache : ICredentialCache
    {
        #region public methods
        public abstract bool IsCertificated(string url);

        public abstract void AddCertification(string url, string userName, string password);

        public abstract NetworkCredential GetNetworkCertification(string url);
        #endregion
    }
}