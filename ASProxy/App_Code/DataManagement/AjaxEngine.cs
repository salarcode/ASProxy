using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace SalarSoft.ASProxy
{
	/// <summary>
	/// AJAX Engine
	/// </summary>
	public class AjaxEngine
	{
		public struct AjaxRequestInfo
		{
			public NameValueCollection HeaderCollection;
			public string ContentType;
			public string RequestUrl;
			public string RequestMethod;
			public Stream InputStream;
			public string PostedFormData;
			public string Referrer;
			public DateTime IfModifiedSince;

			public string Username;
			public string Password;
			public override string ToString()
			{
				return RequestMethod + " " + RequestUrl;
			}
		}

		public struct AjaxResponseInfo
		{
			public string ContentType;
			public Encoding ContentEncoding;
			public string ResponseUrl;
			public long ContentLength;

			public override string ToString()
			{
				return ContentType + " " + ResponseUrl;
			}
		}

		#region consts
		const string _DefaultContentType = "text/plain; charset=utf-8";
		const string _AJAXHeader_ItemsSeperator = "|^|";
		const string _AJAXHeader_ValuesSeperator = "|#|";

		#endregion

		#region local variables
		HttpContext _CurrentContext;
		WebDataCore _WebData;
		private OptionsType _Options;
		private Exception _LastException = null;
		private LastActivityStatus _LastStatus = LastActivityStatus.Normal;
		private string _LastErrorMessage = "";
		public AjaxRequestInfo RequestInfo = new AjaxRequestInfo();
		public AjaxResponseInfo ResponseInfo = new AjaxResponseInfo();
		#endregion

		#region properties
		public LastActivityStatus LastStatus
		{
			get { return _LastStatus; }
			set { _LastStatus = value; }
		}
		public OptionsType Options
		{
			get { return _Options; }
			set { _Options = value; }
		}
		public Exception LastException
		{
			get { return _LastException; }
			set { _LastException = value; }
		}
		public string LastErrorMessage
		{
			get { return _LastErrorMessage; }
			set { _LastErrorMessage = value; }
		}
		#endregion

		#region public methods
		public AjaxEngine(HttpContext currentContext)
		{
			_CurrentContext = currentContext;
		}

		public void Initialize()
		{
			ApplyRequestInfo();
		}

		public void Execute()
		{
			InternalExecute();
			ApplyResponseInfo();
		}
		public void ApplyToResponse(HttpResponse httpResponse)
		{
			// Clear the past content
			httpResponse.ClearContent();

			// set content type
			httpResponse.ContentType = ResponseInfo.ContentType;

			// Set response content type 
			httpResponse.ContentEncoding = ResponseInfo.ContentEncoding;
			//httpResponse.HeaderEncoding = response.HeaderEncoding;

			// applying headers
			if (_WebData != null && _WebData.ResponseInfo.Headers != null)
			{
				WebHeaderCollection coll = _WebData.ResponseInfo.Headers;
				for (int i = 0; i < coll.Count; i++)
				{
					try
					{
						httpResponse.AppendHeader(coll.GetKey(i), coll[i]);
					}
					catch { }
				}
			}

			// seeking to the beggining
			if (_WebData.ResponseData.CanSeek)
				_WebData.ResponseData.Seek(0, SeekOrigin.Begin);

			// Writing response
			byte[] buff = _WebData.ResponseData.GetBuffer();
			if (buff.Length > 0)
				httpResponse.OutputStream.Write(buff, 0, buff.Length);

			// State is ok
			_LastStatus = LastActivityStatus.Normal;

			// Close the response
			httpResponse.End();
		}

		/// <summary>
		/// Gets last error status code if available
		/// </summary>
		public HttpStatusCode LastErrorStatusCode()
		{
			if (_LastException != null)
			{
				if (_LastException is WebException)
				{
					WebException webEx = (WebException)_LastException;
					if (webEx.Response != null && webEx.Response is HttpWebResponse)
					{
						return ((HttpWebResponse)webEx.Response).StatusCode;
					}
				}
				else if (_LastException is HttpException)
				{
					return (HttpStatusCode)((HttpException)_LastException).GetHttpCode();
				}
			}
			return HttpStatusCode.InternalServerError;
		}
		#endregion

		#region private methods
		private void InternalExecute()
		{
			// Data executin should be one time!
			if (_WebData == null)
			{
				do
				{
					// If this is auto recection request send request again to new location
					if (_WebData != null && _WebData.ResponseInfo.AutoRedirect)
					{
						string redirLocation = _WebData.ResponseInfo.AutoRedirectLocation;

						if (_WebData.ResponseInfo.AutoRedirectionType == AutoRedirectType.RequestInternal
							|| _WebData.ResponseInfo.AutoRedirectionType == AutoRedirectType.External)
						{
							redirLocation = _WebData.ResponseInfo.AutoRedirectLocation;

							// correcting request type
							// v5 beta 4
							if (_WebData.ResponseInfo.HttpStatusCode == (int)HttpStatusCode.SeeOther)
							{
								RequestInfo.RequestMethod = WebMethods.GET;
							}
						}
						else if (_WebData.ResponseInfo.AutoRedirectionType == AutoRedirectType.ASProxyPages)
						{
							_LastStatus = LastActivityStatus.Error;
							_LastErrorMessage = "Redirection to ASProxy pages is not supported.";
							return;
						}

						// new location for request
						RequestInfo.RequestUrl = redirLocation;
					}


					// initializing new DataCore
					string userAgent = _CurrentContext.Request.UserAgent;
					if (userAgent == null)
						userAgent = GlobalConsts.ASProxyUserAgent;

					if (_WebData != null)
						_WebData.Dispose();

					_WebData = new WebDataCore(RequestInfo.RequestUrl, userAgent);


					// Apply credentials
					if (string.IsNullOrEmpty(RequestInfo.Username) == false)
					{
						string pass = "";
						if (RequestInfo.Password != null)
							pass = RequestInfo.Password;

						// Set as a certificated request
						_WebData.RequestInfo.CertificatRequest(RequestInfo.Username, pass);
					}

					// IfModifiedSince header
					_WebData.RequestInfo.IfModifiedSince = RequestInfo.IfModifiedSince;

					// Stting the referrer
					_WebData.RequestInfo.Referrer = RequestInfo.Referrer;
					_WebData.RequestInfo.ReferrerUsage = ReferrerType.Referrer;

					_WebData.RequestInfo.CustomHeaders = RequestInfo.HeaderCollection;
					_WebData.RequestInfo.RequestMethod = RequestInfo.RequestMethod;
					_WebData.RequestInfo.PostDataString = RequestInfo.PostedFormData;
					_WebData.RequestInfo.ContentType = RequestInfo.ContentType;
					_WebData.RequestInfo.InputStream = RequestInfo.InputStream;
					_WebData.AcceptCookies = _Options.AcceptCookies;
					_WebData.DisplayErrorPageAsResult = false;

					// Execute the request
					_WebData.Execute();

					// If execution failed
					if (_WebData.Status == LastActivityStatus.Error)
					{
						this._LastStatus = LastActivityStatus.Error;
						this._LastErrorMessage = _WebData.ErrorMessage;
						this._LastException = _WebData.LastException;
						return;
					}

				} while (_WebData.ResponseInfo.AutoRedirect);
			}
		}

		private void ApplyRequestInfo()
		{
			RequestInfo.HeaderCollection = ExtractAJAXHeader(_CurrentContext.Request);
			AJAXInfo _AJAXInfo = GetAJAXInfo(_CurrentContext.Request.Params);

			// validate url
			if (string.IsNullOrEmpty(_AJAXInfo.Url))
			{
				_LastErrorMessage = "Invalid AJAX headers.";
				_LastStatus = LastActivityStatus.Error;
				return;
			}


			RequestInfo.RequestMethod = _AJAXInfo.RequestMethod;
			RequestInfo.RequestUrl = _AJAXInfo.Url;
			RequestInfo.Username = _AJAXInfo.UserName;
			RequestInfo.Password = _AJAXInfo.Password;
			RequestInfo.ContentType = GetContentTypeInCollection(RequestInfo.HeaderCollection, _DefaultContentType);
			RequestInfo.Referrer = GetRequestReferer(RequestInfo.HeaderCollection, _CurrentContext.Request, GlobalConsts.ASProxyProjectUrl);
			
            // BUG: v5beta4 custom headers donsn't support at this moment
            //GetHeadersFromRequest(_CurrentContext.Request, RequestInfo.HeaderCollection);
			RequestInfo.IfModifiedSince = GetIfModifiedSinceHeader(_CurrentContext.Request);

			// current request object
			HttpRequest httpRequest = _CurrentContext.Request;

			if (httpRequest != null)
			{
				RequestInfo.InputStream = httpRequest.InputStream;
			}

			// Get posted form data string
			if (WebMethods.IsMethod(RequestInfo.RequestMethod, WebMethods.DefaultMethods.POST))
			{
				if (httpRequest != null)
				{
					RequestInfo.PostedFormData = httpRequest.Form.ToString();

					// Some web sites encodes the url, and we need to decode it.
					// RequestInfo.PostedFormData = HttpUtility.UrlDecode(RequestInfo.PostedFormData);
					RequestInfo.PostedFormData = HttpUtility.HtmlDecode(RequestInfo.PostedFormData);
				}
			}
			else
				RequestInfo.PostedFormData = "";


			if (WebMethods.IsMethod(RequestInfo.RequestMethod, WebMethods.DefaultMethods.GET))
			{
				// Change requested url by posted data
				RequestInfo.RequestUrl = UrlBuilder.AppendAntoherQueries(RequestInfo.RequestUrl, RequestInfo.PostedFormData);
			}
		}

		private void ApplyResponseInfo()
		{
			if (_WebData != null)
			{
				ResponseInfo.ResponseUrl = _WebData.ResponseInfo.ResponseUrl;
				ResponseInfo.ContentType = _WebData.ResponseInfo.ContentType;
				ResponseInfo.ContentLength = _WebData.ResponseInfo.ContentLength;
				ResponseInfo.ContentEncoding = _WebData.ResponseInfo.ContentEncoding;
			}
		}

		private DateTime GetIfModifiedSinceHeader(HttpRequest httpRequest)
		{
			string header = httpRequest.Headers["If-Modified-Since"];
			if (string.IsNullOrEmpty(header))
				return new DateTime();
			try
			{
				return Convert.ToDateTime(header);
			}
			catch (Exception)
			{
				return new DateTime();
			}
		}

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
			string header = httpRequest.Headers[Consts.AJAX_ReferrerHeaders];
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

		/// <summary>
		/// Gets redirecton url address of current page. 
		/// This method is temporary added for redirection cookies problem in version 4.0
		/// </summary>
		/// <returns></returns>
		private string GetRedirectEncodedUrlForThisRequester(string redirectUrl, string currentRequest)
		{
			string currentPage = HttpContext.Current.Request.Url.ToString();
			// Encode redirect page if needed
			if (_Options.EncodeUrl)
			{
				redirectUrl = UrlProvider.EncodeUrl(redirectUrl);
				currentRequest = UrlProvider.EncodeUrl(currentRequest);
			}

			// Apply current page as referrer url for redirect url
			currentPage = UrlBuilder.AddUrlQuery(currentPage, Consts.qRedirect, currentRequest);

			// If address exists in current page address it will automatically replaced
			currentPage = UrlBuilder.AddUrlQuery(currentPage, Consts.qUrlAddress, redirectUrl);

			// Apply decode option
			currentPage = UrlBuilder.AddUrlQuery(currentPage, Consts.qDecode, Convert.ToByte(_Options.EncodeUrl).ToString());

			// If page is marked as posted back, remark it as no post back
			if (currentPage.IndexOf(Consts.qWebMethod) != -1)
				currentPage = UrlBuilder.RemoveQuery(currentPage, Consts.qWebMethod);

			return currentPage;
		}

		private string GetRedirectEncodedUrlForASProxyPages(string asproxyPage, string currentRequest)
		{
			// Encode redirect page if needed
			if (_Options.EncodeUrl)
			{
				currentRequest = UrlProvider.EncodeUrl(currentRequest);
			}

			// Apply current page as referrer url for redirect url
			asproxyPage = UrlBuilder.AddUrlQuery(asproxyPage, Consts.qUrlAddress, currentRequest);

			// Apply decode option
			asproxyPage = UrlBuilder.AddUrlQuery(asproxyPage, Consts.qDecode, Convert.ToByte(_Options.EncodeUrl).ToString());

			// If page is marked as posted back, remark it as no post back
			if (asproxyPage.IndexOf(Consts.qWebMethod) != -1)
				asproxyPage = UrlBuilder.RemoveQuery(asproxyPage, Consts.qWebMethod);

			return asproxyPage;
		}

		#endregion

		#region static methods
		static private void GetHeadersFromRequest(HttpRequest httpRequest, NameValueCollection result)
		{
			string header;

			// BUG:: Not supported
			// GZip and Deflate is not supported by ASProxy yet!
			//header = httpRequest.Headers["Accept-Encoding"];
			//if (string.IsNullOrEmpty(header) == false)
			//    result.Add("Accept-Encoding", header);

			// BUG:: ASP.NET doesn't allow changing If-Modified-Since header directly
			// It should change by property
			//header = httpRequest.Headers["If-Modified-Since"];
			//if (string.IsNullOrEmpty(header) == false)
			//    result.Add("If-Modified-Since", header);

			header = httpRequest.Headers["Accept-Language"];
			if (string.IsNullOrEmpty(header) == false)
				result.Add("Accept-Language", header);

			header = httpRequest.Headers["Cache-Control"];
			if (string.IsNullOrEmpty(header) == false)
				result.Add("Cache-Control", header);

			header = httpRequest.Headers["If-Match"];
			if (string.IsNullOrEmpty(header) == false)
				result.Add("If-Match", header);

			header = httpRequest.Headers["If-None-Match"];
			if (string.IsNullOrEmpty(header) == false)
				result.Add("If-None-Match", header);

			header = httpRequest.Headers["If-Unmodified-Since"];
			if (string.IsNullOrEmpty(header) == false)
				result.Add("If-Unmodified-Since", header);

			header = httpRequest.Headers["Last-Modified"];
			if (string.IsNullOrEmpty(header) == false)
				result.Add("Last-Modified", header);

			header = httpRequest.Headers["Pragma"];
			if (string.IsNullOrEmpty(header) == false)
				result.Add("Pragma", header);

			header = httpRequest.Headers["Warning"];
			if (string.IsNullOrEmpty(header) == false)
				result.Add("Warning", header);

		}

		static AJAXInfo GetAJAXInfo(NameValueCollection queryString)
		{
			AJAXInfo result = new AJAXInfo();
			string query;
			if (UrlProvider.GetRequestQuery(queryString, Consts.qDecode, out query))
			{
				try
				{
					result.Decode = Convert.ToBoolean(Convert.ToInt32(query));
				}
				catch
				{
					result.Decode = false;
				}
			}

			if (UrlProvider.GetRequestQuery(queryString, Consts.qAjaxUrl, out query))
			{
				try
				{
					if (result.Decode)
						result.Url = UrlProvider.DecodeUrl(query);
					else
						result.Url = query;
				}
				catch
				{
					result.Url = null;
				}
			}

			if (UrlProvider.GetRequestQuery(queryString, Consts.qWebMethod, out query))
			{
				// Removes invalid characters in method
				result.RequestMethod = WebMethods.OmitInvalidCharacters(query);
			}
			else
			{
				result.RequestMethod = WebMethods.GET;
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
			string asproxyAJAX = request.Headers[Consts.AJAX_Headers];

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
	}

	public struct AJAXInfo
	{
		public bool Decode;
		public string RequestMethod;
		public string Url;
		public string UserName;
		public string Password;
		public string Referrer;
	}

}