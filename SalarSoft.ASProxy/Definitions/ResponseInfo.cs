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
