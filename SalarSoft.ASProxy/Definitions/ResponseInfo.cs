using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SalarSoft.ASProxy
{
	public class EngineResponseInfo
	{
		private MimeContentType _contentTypeMime = MimeContentType.application;
		private string _contentType;
		#region properties
		public string ResponseUrl;
		public long ContentLength;
		public Encoding ContentEncoding;
		public string ContentFilename;

		public string HtmlDocType;
		public string ExtraCodesForPage;
		public string ExtraCodesForBody;
		public string HtmlPageTitle;
		public bool HtmlIsFrameSet;

		public string ContentType
		{
			get
			{
				return _contentType;
			}
			set
			{
				_contentType = value;
				_contentTypeMime = Common.StringToContentType(_contentType);
			}
		}

		public MimeContentType ContentTypeMime
		{
			get
			{
				return _contentTypeMime;
			}
		}
		#endregion
	}

	public class WebDataResponseInfo
	{
		private MimeContentType _contentTypeMime = MimeContentType.application;
		private string _contentType;
		#region properties
		public bool AutoRedirect;
		public AutoRedirectType AutoRedirectionType;
		public string AutoRedirectLocation;
		public long ContentLength;
		public bool IsHttpRequest;
		public string ContentFilename;
		public string ResponseUrl;
		public string ResponseRootUrl;
		public Encoding ContentEncoding;
		public string HttpStatusDescription;
		public int HttpStatusCode;
		public CookieCollection Cookies;
		public WebHeaderCollection Headers;
		public bool RangeResponse;
		public int RangeBegin;
		public long RangeEnd;
		public InternetProtocols ResponseProtocol;

		public string ContentType
		{
			get
			{
				return _contentType;
			}
			set
			{
				_contentType = value;
				_contentTypeMime = Common.StringToContentType(_contentType);
			}
		}

		public MimeContentType ContentTypeMime
		{
			get
			{
				return _contentTypeMime;
			}
		}
		#endregion

		#region public methods
		public override string ToString()
		{
			return HttpStatusCode + " " + ResponseUrl;
		}
		#endregion
	}
}
