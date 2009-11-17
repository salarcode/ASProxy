using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Net;
using System.IO;

namespace SalarSoft.ASProxy
{

	public class EngineRequestInfo
	{
		#region varables
		private string _contentType;
		private MimeContentType _contentTypeMime;
		#endregion

		#region properties
		public bool BufferResponse = true;

		/// <summary>
		/// Determines if requested url should be sent as referrer to back-end request
		/// </summary>
		public bool RequestUrlAsReferrer;

		/// <summary>
		/// If enabled the error pages will be shown due an error
		/// </summary>
		public bool PrrocessErrorPage;

		/// <summary>
		/// Requested method
		/// </summary>
		public string RequestMethod;
		public string RequestUrl;
		public RequesterType RequesterType;

		/// <summary>
		/// Posted data in string format
		/// </summary>
		public string PostDataString;

		/// <summary>
		/// behaves like referrer
		/// </summary>
		public string RedirectedFrom;

		/// <summary>
		/// Post back data
		/// </summary>
		public Stream InputStream;
		public DateTime IfModifiedSince;

		/// <summary>
		/// Custom headers to send to back-end request
		/// </summary>
		public NameValueCollection CustomHeaders;

		public bool RangeRequest;
		public int RangeBegin;
		public long RangeEnd;

		/// <summary>
		/// Requested content type
		/// </summary>
		public string ContentTypeString
		{
			get { return _contentType; }
		}

		/// <summary>
		/// Requested content type
		/// </summary>
		public MimeContentType ContentTypeMime
		{
			get { return _contentTypeMime; }
		}

		#endregion

		#region public methods
		public override string ToString()
		{
			return RequestMethod + " " + RequestUrl;
		}
		public void SetContentType(string contentType)
		{
			_contentType = contentType;
			_contentTypeMime = Common.StringToContentType(contentType);
		}
		public void SetContentType(MimeContentType contentType)
		{
			_contentTypeMime = contentType;
			_contentType = Common.ContentTypeToString(contentType);
		}
		#endregion
	}

	public class WebDataRequestInfo
	{
		#region variables
		private string _Username;
		private string _Password;
		#endregion

		#region properties
		public bool BufferResponse = true;

		public DateTime IfModifiedSince;
		public Stream InputStream;
		public ReferrerType ReferrerUsage;
		public CookieCollection Cookies;
		public NameValueCollection CustomHeaders;
		public RequesterType RequesterType;
		public string PostDataString;
		public string RequestUrl;
		public string ContentType;
		public string UserAgent;
		public string Referrer;
		public string RequestMethod;
		public bool AcceptCookies;
		public bool TempCookies;
		public bool IsCertificated;
		public bool PrrocessErrorPage;

		public bool RangeRequest;
		public int RangeBegin;
		public long RangeEnd;
		#endregion

		#region public methods
		public NetworkCredential GetCertification()
		{
			return new NetworkCredential(_Username, _Password);
		}

		public void CertificatRequest(string username, string password)
		{
			_Username = username;
			_Password = password;
			IsCertificated = true;
		}

		public override string ToString()
		{
			return RequestMethod + " " + RequestUrl;
		}
		#endregion
	}
}
