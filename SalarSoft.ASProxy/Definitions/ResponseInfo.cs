using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SalarSoft.ASProxy
{
    public class EngineResponseInfo
    {
        #region properties
        public string ResponseUrl;
        public long ContentLength;
        public string ContentType;
        public Encoding ContentEncoding;
        public string ContentFilename;

        public string HtmlDocType;
        public string HtmlInitilizerCodes;
        public string HtmlPageTitle;
        public bool HtmlIsFrameSet;
        #endregion
    }

    public class WebDataResponseInfo
    {
        #region properties
        public bool AutoRedirect;
        public AutoRedirectType AutoRedirectionType;
        public string AutoRedirectLocation;
        public long ContentLength;
        public bool IsHttpRequest;
        public string ContentFilename;
        public string ResponseUrl;
        public string ResponseRootUrl;
        public string ContentType;
        public Encoding ContentEncoding;
        public string HttpStatusDescription;
        public int HttpStatusCode;
        public CookieCollection Cookies;
        public WebHeaderCollection Headers;
		public bool RangeResponse;
		public int RangeBegin;
		public long RangeEnd;
		public InternetProtocols ResponseProtocol;
		#endregion

        #region public methods
        public override string ToString()
        {
            return HttpStatusCode + " " + ResponseUrl;
        }
        #endregion
    }
}
