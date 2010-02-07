using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using SalarSoft.ASProxy.Exposed;

namespace SalarSoft.ASProxy.BuiltIn
{
    /// <summary>
    /// AJAX Engine provider
    /// </summary>
    public class AjaxEngine : ASProxyEngine
    {
        #region consts
        const string _DefaultContentType = "text/plain; charset=utf-8";
        const string _AJAXHeader_ItemsSeperator = "|^|";
        const string _AJAXHeader_ValuesSeperator = "|#|";
        #endregion

        #region local variables
        HttpContext _currentContext;
        #endregion

        #region properties
        public AjaxRequestInfo AjaxRequest
        {
            get { return (AjaxRequestInfo)RequestInfo; }
            set { RequestInfo = value; }
        }
        public AjaxResponseInfo AjaxResponse
        {
            get { return (AjaxResponseInfo)ResponseInfo; }
            set { ResponseInfo = value; }
        }
        #endregion

        #region public methods
        public AjaxEngine()
        {
            RequestInfo = new AjaxRequestInfo();
            ResponseInfo = new AjaxResponseInfo();
        }

        public override void Initialize(HttpRequest httpRequest)
        {
            _currentContext = HttpContext.Current;
            base.Initialize(httpRequest);
        }

        public override void Initialize(string requestUrl)
        {
            // not supported in this plug in
            throw new NotSupportedException();
        }
        #endregion

        #region protected
        protected override void IntializeRequestInfo(HttpRequest httpRequest)
        {
            // base method
            base.IntializeRequestInfo(httpRequest);

            AjaxRequest.CustomHeaders = ExtractAJAXHeader(_currentContext.Request);
            AJAXInfo _AJAXInfo = GetAJAXInfo(_currentContext.Request.Params);

            // validate url
            if (string.IsNullOrEmpty(RequestInfo.RequestUrl))
            {
                LastErrorMessage = "Invalid AJAX headers.";
                LastStatus = LastStatus.Error;
                return;
            }

            AjaxRequest.Username = _AJAXInfo.UserName;
            AjaxRequest.Password = _AJAXInfo.Password;

            RequestInfo.SetContentType(GetContentTypeInCollection(AjaxRequest.CustomHeaders, _DefaultContentType));
            RequestInfo.RedirectedFrom = GetRequestReferer(
                                            AjaxRequest.CustomHeaders,
                                            _currentContext.Request,
                                            Consts.BackEndConenction.ASProxyProjectUrl);
        }
        protected override void ApplyWebDataRequestInfo(IWebData webData)
        {
			// call base emthod
            base.ApplyWebDataRequestInfo(webData);

            // ajax headers
            webData.RequestInfo.CustomHeaders = AjaxRequest.CustomHeaders;

            // Apply credentials
            if (string.IsNullOrEmpty(AjaxRequest.Username) == false)
            {
                string pass = "";
                if (AjaxRequest.Password != null)
                    pass = AjaxRequest.Password;

                // Set as a certificated request
                webData.RequestInfo.CertificatRequest(AjaxRequest.Username, pass);
            }

        }

        protected override void ExecuteToResponse_ApplyResponseInfo(HttpResponse httpResponse)
        {
            base.ExecuteToResponse_ApplyResponseInfo(httpResponse);
        }
        public override void ExecuteHandshake()
        {
            base.ExecuteHandshake();
        }
        public override void ExecuteToResponse(HttpResponse httpResponse)
        {
            base.ExecuteToResponse(httpResponse);
        }

        public override string ExecuteToString()
        {
            throw new NotSupportedException();
        }
        public override void ExecuteToStream(Stream stream)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region private methods
        private string GetContentTypeInCollection(NameValueCollection coll, string defaultVal)
        {
            foreach (string key in coll)
            {
                if (key.ToLower() == "content-type")
                {
                    return coll[key];
                }
            }
            return defaultVal;
        }

        private string GetRequestReferer(NameValueCollection coll, HttpRequest httpRequest, string defaultVal)
        {
            // first of all trying to get ftom sent header
            string header = httpRequest.Headers[Consts.BackEndConenction.AJAX_ReferrerHeaders];
            if (string.IsNullOrEmpty(header) == false)
                return header.Trim();

            // second, trying to get from ajax headers
            foreach (string key in coll)
            {
                if (key.ToLower() == "referer")
                {
                    return coll[key];
                }
            }


            if (httpRequest.UrlReferrer != null)
                return httpRequest.UrlReferrer.ToString();

            return defaultVal;
        }

        #endregion

        #region static methods
        private AJAXInfo GetAJAXInfo(NameValueCollection queryString)
        {
            AJAXInfo result = new AJAXInfo();
            string query;


            // Get requested url
            // We are using "ajaxurl" intead of simple "url", because some implementations of ajax use "url"
            // and that causes some issues.
            string url = queryString[Consts.Query.AjaxUrlAddress];

            // if url is provided
            if (!string.IsNullOrEmpty(url))
            {
                bool tmpBool = false;

                string decode = queryString[Consts.Query.Decode];
                if (!string.IsNullOrEmpty(decode))
                {
                    try
                    {
                        tmpBool = Convert.ToBoolean(Convert.ToInt32(decode));
                    }
                    catch
                    {
                        tmpBool = false;
                    }
                }

                // If url is encoded, decode it
                if (tmpBool)
                    url = UrlProvider.DecodeUrl(url);

                RequestInfo.RequestUrl = url;
            }

            if (UrlProvider.GetRequestQuery(queryString, "pas", out query))
            {
                try
                {
                    result.Password = UrlProvider.DecodeUrl(query);
                }
                catch
                {
                    result.Password = null;
                }
            }

            if (UrlProvider.GetRequestQuery(queryString, "use", out query))
            {
                try
                {
                    result.UserName = UrlProvider.DecodeUrl(query);
                }
                catch
                {
                    result.UserName = null;
                }
            }

            return result;
        }

        static NameValueCollection ExtractAJAXHeader(HttpRequest request)
        {
            string asproxyAJAX = request.Headers[Consts.BackEndConenction.AJAX_Headers];

            NameValueCollection coll = new NameValueCollection();
            if (asproxyAJAX == null)
                return coll;

            asproxyAJAX = HtmlTags.RemoveBraketsFromStartEnd(asproxyAJAX.Trim());

            string[] parts = asproxyAJAX.Split(new string[] { _AJAXHeader_ItemsSeperator }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                string[] item = parts[i].Split(new string[] { _AJAXHeader_ValuesSeperator }, StringSplitOptions.RemoveEmptyEntries);

                // Items count should be 2
                if (item.Length > 1)
                {
                    string key = HtmlTags.RemoveQuotesFromStartEnd(item[0]);
                    string value = HtmlTags.RemoveQuotesFromStartEnd(item[1]);
                    coll.Add(key, value);
                }
            }
            return coll;
        }
        #endregion


        public class AjaxRequestInfo : EngineRequestInfo
        {
            public string Username;
            public string Password;
            public override string ToString()
            {
                return RequestMethod + " " + RequestUrl;
            }
        }

        public class AjaxResponseInfo : EngineResponseInfo
        {
            public override string ToString()
            {
                return ContentType + " " + ResponseUrl;
            }
        }
        public struct AJAXInfo
        {
            public bool Decode;
            public string UserName;
            public string Password;
            public string Referrer;
        }
    }
}