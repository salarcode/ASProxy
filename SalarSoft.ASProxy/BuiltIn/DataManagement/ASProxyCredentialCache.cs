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
        }

        public void AddCertification(HttpContext context, string url, string userName, string password)
        {
            if (context.Session == null)
                return;

            string key = GetCertificatedKey(url);
            CredentialDetails details = new CredentialDetails();
            details.UserName = userName;
            details.Password = password;
            context.Session[key] = details;
        }

        public override NetworkCredential GetNetworkCertification(string url)
        {
            return GetNetworkCertification(GetCertification(url));
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