using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Net;

namespace SalarSoft.ASProxy.Exposed
{
    /// <summary>
    /// Generalize the CredentialCache
    /// </summary>
    public interface ICredentialCache
    {
        /// <summary>
        /// If the requested url has a authorized credentials
        /// </summary>
        bool IsCertificated(string url);

        /// <summary>
        /// Authorized the credentials for an Url
        /// </summary>
        void AddCertification(string url, string userName, string password);

        /// <summary>
        /// Reads the authorized credentials
        /// </summary>
        NetworkCredential GetNetworkCertification(string url);
    }
}