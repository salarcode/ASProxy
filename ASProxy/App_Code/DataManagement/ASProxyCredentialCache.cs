using System;
using System.Net;
using System.Web;
using System.Web.SessionState;

namespace SalarSoft.ASProxy
{
	public class ASProxyCredentialCache
	{
		class CredentialDetails
		{
			public string UserName;
			public string Password;
		}

		public static bool IsCertificated(string url)
		{
			if (HttpContext.Current == null || HttpContext.Current.Session == null)
				return false;

			string key=GetCertificatedKey(url);
			return (HttpContext.Current.Session[key]!=null);
		}

		public static void AddCertification(string url, string userName, string password)
		{
			HttpSessionState session = HttpContext.Current.Session;
			if (session == null) return;

			string key = GetCertificatedKey(url);
			CredentialDetails details = new CredentialDetails();
			details.UserName = userName;
			details.Password = password;
			session[key] = details;
		}

		public static void AddCertification(HttpContext context, string url, string userName, string password)
		{
			if (context.Session == null)
				return;

			string key = GetCertificatedKey(url);
			CredentialDetails details = new CredentialDetails();
			details.UserName = userName;
			details.Password = password;
			context.Session[key] = details;
		}

		public static NetworkCredential GetNetworkCertification(string url)
		{
			return GetNetworkCertification(GetCertification(url));
		}

		private static NetworkCredential GetNetworkCertification(CredentialDetails credit)
		{
			return new NetworkCredential(credit.UserName, credit.Password);
		}
	
		private static CredentialDetails GetCertification(string url)
		{
			string key = GetCertificatedKey(url);
			return (CredentialDetails)HttpContext.Current.Session[key];
		}

		private static string GetCertificatedKey(string url)
		{
			Uri reqUri = new Uri(url);
			return reqUri.Scheme + reqUri.Port + reqUri.Host;
		}

	}
}