using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace SalarSoft.ASProxy
{
	/// <summary>
	/// ASProxy Engine
	/// </summary>
	public class ASProxyEngine : IDisposable
	{
		public struct EngineRequestInformation
		{
			private MimeContentType fContentType;
			private string fContentTypeString;

			/// <summary>
			/// Request method for .......
			/// </summary>
			public string RequestMethod;

			public MimeContentType ContentType
			{
				get { return fContentType; }
				set
				{
					fContentType = value;
					fContentTypeString = Common.ContentTypeToString(fContentType);
				}
			}
			public string ContentTypeString
			{
				get { return fContentTypeString; }
				set
				{
					fContentTypeString = value;
					fContentType = Common.StringToContentType(fContentTypeString);
				}
			}
			public string RequestUrl;
			public string RequestedQueries;
			public Stream InputStream;
			public string PostedFormData;
			public string RedirectedFrom;
			public DateTime IfModifiedSince;
			public NameValueCollection CustomHeaders;

			public override string ToString()
			{
				return RequestMethod + " " + RequestUrl;
			}
		}

		public struct EngineResponseInformation
		{
			public bool IsFrameSet;
			public string DocType;
			public string ResponseUrl;
			public string PageInitilizerCodes;
			public long ContentLength;
			public string ContentType;
			public string ContentCharset;
			public string ContentPageTitle;
			public Encoding ContentEncoding;

			public override string ToString()
			{
				return ResponseUrl + " " + ContentPageTitle;
			}
		}

		#region Private variables
		private bool _AutoDetection = true;
		private bool _UseRequestUrlAsReferrer = false;
		private bool _UseFileNameInHeader = false;
		private string _HttpRequestMethod;
		private OptionsType _Options;
		private WebDataCore _WebData;
		private Exception _LastException = null;
		private ProcessTypeForData _ProcessTypeForData = ProcessTypeForData.None;
		private LastActivityStatus _LastStatus = LastActivityStatus.Normal;
		public string _LastErrorMessage = "";
		#endregion

		#region Properties
		public EngineRequestInformation RequestInfo;
		public EngineResponseInformation ResponseInfo;
		public bool AutoDetection
		{
			get { return _AutoDetection; }
			set { _AutoDetection = value; }
		}
		public OptionsType Options
		{
			get { return _Options; }
			set { _Options = value; }
		}
		public WebDataCore WebData
		{
			get { return _WebData; }
			set { _WebData = value; }
		}
		public LastActivityStatus LastStatus
		{
			get { return _LastStatus; }
			set { _LastStatus = value; }
		}
		public Exception LastException
		{
			get { return _LastException; }
			set { _LastException = value; }
		}
		public ProcessTypeForData ProcessTypeForData
		{
			get { return _ProcessTypeForData; }
			set { _ProcessTypeForData = value; }
		}
		public bool UseRequestUrlAsReferrer
		{
			get { return _UseRequestUrlAsReferrer; }
			set { _UseRequestUrlAsReferrer = value; }
		}
		public string LastErrorMessage
		{
			get { return _LastErrorMessage; }
			set { _LastErrorMessage = value; }
		}
		public bool UseFileNameInHeader
		{
			get { return _UseFileNameInHeader; }
			set { _UseFileNameInHeader = value; }
		}
		#endregion

		#region Constructor
		public ASProxyEngine(ProcessTypeForData processTypeForData)
		{
			_ProcessTypeForData = processTypeForData;

			// Get the content type string for future uses.
			RequestInfo.ContentTypeString = GetContentTypeStringByProcessType(processTypeForData);
		}
		public ASProxyEngine(ProcessTypeForData processTypeForData, bool loadCookieOptions)
		{
			_ProcessTypeForData = processTypeForData;

			// Load options
			if (loadCookieOptions)
				_Options = ASProxyConfig.GetCookieOptions();

			// Get the content type string for future uses.
			RequestInfo.ContentTypeString = GetContentTypeStringByProcessType(processTypeForData);
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Initialize ASProxy engine using HttpRequest instance.
		/// </summary>
		/// <param name="httpRequest">HttpRequest instance</param>
		public void Initialize(HttpRequest httpRequest)
		{
			InitializeRequestQueriesByHttpRequest(httpRequest);
			DoAutoDetection(httpRequest);
		}

		/// <summary>
		/// Initialize ASProxy engine using specified url address.
		/// </summary>
		/// <param name="requestUrl">Request url address</param>
		public void Initialize(string requestUrl)
		{
			RequestInfo.RequestUrl = requestUrl;
			RequestInfo.RequestedQueries = requestUrl;
			RequestInfo.RequestMethod = WebMethods.GET;
			DoAutoDetection(null);
		}

		/// <summary>
		/// Executes the request and gets the responses but doesn't process the results.
		/// </summary>
		public void PreExecution()
		{
			if (_AutoDetection)
			{
				// If this is an image request, we should provide orginal link as referer
				if (RequestInfo.ContentType == MimeContentType.image_gif || RequestInfo.ContentType == MimeContentType.image_jpeg)
					this._UseRequestUrlAsReferrer = true;
			}

			// Data executin should be one time!
			if (_WebData == null)
			{
				do
				{
					// If this is auto recection request send request again to new location
					if (_WebData != null && _WebData.ResponseInfo.AutoRedirect)
					{
						if (_WebData.ResponseInfo.AutoRedirectionType == AutoRedirectType.RequestInternal || _WebData.ResponseInfo.AutoRedirectionType == AutoRedirectType.External)
						{
							string autoRedirectLocation = _WebData.ResponseInfo.AutoRedirectLocation;

							// This redirections is temporary added for redirection cookies problem in version 4.0
							string redir = GetRedirectEncodedUrlForThisRequester(autoRedirectLocation, RequestInfo.RequestUrl);
							HttpContext.Current.Response.Redirect(redir);
						}
						else if (_WebData.ResponseInfo.AutoRedirectionType == AutoRedirectType.ASProxyPages)
						{
							string redir = GetRedirectEncodedUrlForASProxyPages(_WebData.ResponseInfo.AutoRedirectLocation, RequestInfo.RequestUrl);
							HttpContext.Current.Response.Redirect(redir);
						}
						else if (_WebData.ResponseInfo.AutoRedirectionType == AutoRedirectType.External)
						{
							// External redirections not implemented yet
						}

						// Exit the method
						return;
					}
					else
					{
						// initializing new DataCore
						string userAgent = HttpContext.Current.Request.UserAgent;
						if (string.IsNullOrEmpty(userAgent))
							userAgent = GlobalConsts.ASProxyUserAgent;

						if (_WebData != null)
							_WebData.Dispose();

						_WebData = new WebDataCore(RequestInfo.RequestUrl, userAgent);
					}

					// Stting the referrer
					if (string.IsNullOrEmpty(RequestInfo.RedirectedFrom) == false)
					{
						_WebData.RequestInfo.Referrer = RequestInfo.RedirectedFrom;
						_WebData.RequestInfo.ReferrerUsage = ReferrerType.Referrer;
					}
					else
					{
						if (_UseRequestUrlAsReferrer)
							_WebData.RequestInfo.ReferrerUsage = ReferrerType.RequesterAsReferrer;
						else
							_WebData.RequestInfo.ReferrerUsage = ReferrerType.None;
					}

					_WebData.RequestInfo.IfModifiedSince = RequestInfo.IfModifiedSince;
					_WebData.RequestInfo.CustomHeaders = RequestInfo.CustomHeaders;
					_WebData.RequestInfo.RequestMethod = RequestInfo.RequestMethod;
					_WebData.RequestInfo.PostDataString = RequestInfo.PostedFormData;
					_WebData.RequestInfo.ContentType = RequestInfo.ContentTypeString;
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

				SetResponseInformation();
			}
		}


		/// <summary>
		/// Execute the request and apply the results directly to response.
		/// </summary>
		public void Execute(HttpResponse response)
		{
			bool useStringResult;
			string stringResult = "";
			MemoryStream streamResult = null;

			// If request does not need any DataProcessor command we should use stream for better results
			if (_ProcessTypeForData == ProcessTypeForData.None)
				useStringResult = false;
			else
				useStringResult = true;

			// Run string or stream result
			if (useStringResult)
				Execute(out stringResult);
			else
				Execute(out streamResult);

			// If some error occurs
			if (_LastStatus == LastActivityStatus.Error)
				return;

			// Clear the past content
			response.ClearContent();

			// Clear special headers
			// BUGFIX: There may be some cookies! I shouldn't remove them here.
			// Since v5.0
			// Common.ClearASProxyRespnseHeader(response);

			// set content type
			response.ContentType = ResponseInfo.ContentType;

			if (_UseFileNameInHeader)
				AddFileNameToHeader(response, RequestInfo.RequestUrl);

			// Set reponse additional headers
			ApplyHeaders(response);

			// Add content type header
			response.AddHeader("Content-Type", ResponseInfo.ContentType);
			
			// ASProxy signature in request header
			response.AddHeader("X-Powered-By", GlobalConsts.ASProxyAgentVersion);
			response.AddHeader("X-Working-With", GlobalConsts.ASProxyAgentVersion);


			// Set response content type 
			response.Charset = ResponseInfo.ContentCharset;
			response.ContentEncoding = ResponseInfo.ContentEncoding;
			response.HeaderEncoding = response.HeaderEncoding;

			response.Write(ResponseInfo.PageInitilizerCodes);

			if (useStringResult)
			{
				// Write string result 
				response.Write(stringResult);
			}
			else
			{
				// Write stream to reponse
				streamResult.WriteTo(response.OutputStream);
				streamResult.Close();
			}

			// State is ok
			_LastStatus = LastActivityStatus.Normal;

			// Close the response
			response.End();
		}

		public void Execute(out MemoryStream responseStream)
		{
			_LastStatus = LastActivityStatus.Normal;

			responseStream = null;

			try
			{
				PreExecution();

				if (this._LastStatus == LastActivityStatus.Error)
					return;

				string executed;
				switch (this._ProcessTypeForData)
				{
					case ProcessTypeForData.HTML:
						HtmlProcessor htmlProcessor = new HtmlProcessor(_WebData);
						htmlProcessor.Options = _Options;
						executed = htmlProcessor.Execute();

						ResponseInfo.ContentPageTitle = htmlProcessor.PageTitle;
						ResponseInfo.ContentEncoding = htmlProcessor.PageEncoding;
						ResponseInfo.ContentCharset = htmlProcessor.PageEncoding.BodyName;
						ResponseInfo.IsFrameSet = htmlProcessor.HtmlIsFrameSet;
						ResponseInfo.PageInitilizerCodes = htmlProcessor.PageInitializerCodes;
						ResponseInfo.DocType = htmlProcessor.DocType;
						ResponseInfo.ContentLength = executed.Length;

						executed += "<!--This is ASProxy that powered by salarsoft-->     ";
						responseStream = new MemoryStream(htmlProcessor.PageEncoding.GetBytes(executed));

						// log this activity
						if (LogSystem.Enabled)
							LogSystem.Log(LogEntity.UrlRequested, HttpContext.Current.Request, ResponseInfo.ResponseUrl, ResponseInfo.ContentPageTitle);
						break;

					case ProcessTypeForData.JavaScript:
						JSProcessor jsProcessor = new JSProcessor(_WebData);
						jsProcessor.Options = _Options;
						executed = jsProcessor.Execute();

						ResponseInfo.ContentEncoding = jsProcessor.ResponsePageEncoding;
						ResponseInfo.ContentCharset = jsProcessor.ResponsePageEncoding.BodyName;
						ResponseInfo.ContentLength = executed.Length;

						executed += " /** This is ASProxy that powered by salarsoft **/      ";
						responseStream = new MemoryStream(jsProcessor.ResponsePageEncoding.GetBytes(executed));
						break;

					case ProcessTypeForData.CSS:
						CSSProcessor cssProcessor = new CSSProcessor(_WebData);
						cssProcessor.Options = _Options;
						executed = cssProcessor.Execute();

						ResponseInfo.ContentEncoding = cssProcessor.ResponsePageEncoding;
						ResponseInfo.ContentCharset = cssProcessor.ResponsePageEncoding.BodyName;
						ResponseInfo.ContentLength = executed.Length;

						executed += " /** This is ASProxy that powered by salarsoft **/     ";
						responseStream = new MemoryStream(cssProcessor.ResponsePageEncoding.GetBytes(executed));
						break;

					default:
						responseStream = new MemoryStream(_WebData.ResponseData.GetBuffer());
						break;
				}
			}
			catch (ThreadAbortException)
			{
			}
			finally
			{
			}
		}

		public void Execute(out string response)
		{
			_LastStatus = LastActivityStatus.Normal;
			response = "";

			try
			{
				PreExecution();

				if (this._LastStatus == LastActivityStatus.Error)
					return;

				string executed;
				switch (this._ProcessTypeForData)
				{
					case ProcessTypeForData.HTML:
						HtmlProcessor htmlProcessor = new HtmlProcessor(_WebData);
						htmlProcessor.Options = _Options;
						executed = htmlProcessor.Execute();

						ResponseInfo.ContentPageTitle = htmlProcessor.PageTitle;
						ResponseInfo.ContentEncoding = htmlProcessor.PageEncoding;
						ResponseInfo.ContentCharset = htmlProcessor.PageEncoding.BodyName;
						ResponseInfo.IsFrameSet = htmlProcessor.HtmlIsFrameSet;
						ResponseInfo.PageInitilizerCodes = htmlProcessor.PageInitializerCodes;
						ResponseInfo.DocType = htmlProcessor.DocType;
						ResponseInfo.ContentLength = executed.Length;

						response = executed;// +"<!--This is ASProxy that powered by salarsoft-->     ";

						// log this activity
						if (LogSystem.Enabled)
							LogSystem.Log(LogEntity.UrlRequested, HttpContext.Current.Request, ResponseInfo.ResponseUrl, ResponseInfo.ContentPageTitle);
						break;

					case ProcessTypeForData.JavaScript:
						JSProcessor jsProcessor = new JSProcessor(_WebData);
						jsProcessor.Options = _Options;
						executed = jsProcessor.Execute();

						ResponseInfo.ContentEncoding = jsProcessor.ResponsePageEncoding;
						ResponseInfo.ContentCharset = jsProcessor.ResponsePageEncoding.BodyName;
						ResponseInfo.ContentLength = executed.Length;

						response = executed;// +" /** This is ASProxy that powered by salarsoft **/      ";
						break;

					case ProcessTypeForData.CSS:
						CSSProcessor cssProcessor = new CSSProcessor(_WebData);
						cssProcessor.Options = _Options;
						executed = cssProcessor.Execute();

						ResponseInfo.ContentEncoding = cssProcessor.ResponsePageEncoding;
						ResponseInfo.ContentCharset = cssProcessor.ResponsePageEncoding.BodyName;
						ResponseInfo.ContentLength = executed.Length;

						response = executed;// +" /** This is ASProxy that powered by salarsoft **/     ";
						break;

					default:
						// Default is corrected since V4.7
						Encoding contentEncode;
						response = Processors.GetString(_WebData, _Options.IgnorePageEncoding, out contentEncode);

						ResponseInfo.ContentEncoding = contentEncode;
						ResponseInfo.ContentCharset = contentEncode.BodyName;
						ResponseInfo.ContentLength = response.Length;

						// old method
						//response = System.Text.Encoding.UTF8.GetString(_WebData.ResponseData.GetBuffer());
						break;
				}

			}
			catch (ThreadAbortException)
			{
			}
			finally
			{
			}
		}

		public void ApplyHeaders(HttpResponse response)
		{

			if (_WebData != null && _WebData.ResponseInfo.Headers != null)
			{
				WebHeaderCollection coll = _WebData.ResponseInfo.Headers;
				for (int i = 0; i < coll.Count; i++)
				{
					try
					{
						response.AppendHeader(coll.GetKey(i), coll[i]);
					}
					catch { }
				}
			}
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

		public void Dispose()
		{
			if (_WebData != null)
				_WebData.Dispose();
			_WebData = null;
		}
		#endregion

		#region Private methods
		private NameValueCollection GetHeadersFromRequest(HttpRequest httpRequest)
		{
			NameValueCollection result = new NameValueCollection();

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

			string header;
			header = httpRequest.Headers["Accept-Encoding"];
			if (string.IsNullOrEmpty(header) == false)
				result.Add("Accept-Encoding", header);

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

			return result;
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

		/// <summary>
		/// 
		/// </summary>
		/// <Added>Since V4.1</Added>
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


		private string GetContentTypeStringByProcessType(ProcessTypeForData processTypeForData)
		{
			MimeContentType noType = (MimeContentType)(-1);
			MimeContentType cType = noType;
			switch (processTypeForData)
			{
				case ProcessTypeForData.CSS:
					cType = MimeContentType.text_css;
					break;

				case ProcessTypeForData.HTML:
					cType = MimeContentType.text_html;
					break;

				case ProcessTypeForData.JavaScript:
					cType = MimeContentType.text_javascript;
					break;
			}
			if (cType != noType)
				return Common.ContentTypeToString(cType);
			else
				return "";
		}


		private void AddFileNameToHeader(HttpResponse response, string name)
		{
			const string HTTP_HEADER_Content_Disposition = "Content-Disposition";
			string Content_Disposition_File = "attachment; filename=" + VirtualPathUtility.GetFileName(name) + "";

			response.AddHeader(HTTP_HEADER_Content_Disposition, Content_Disposition_File);
		}



		/// <summary>
		/// Set response information
		/// </summary>
		private void SetResponseInformation()
		{
			if (_WebData != null)
			{
				ResponseInfo.ResponseUrl = _WebData.ResponseInfo.ResponseUrl;
				ResponseInfo.ContentType = _WebData.ResponseInfo.ContentType;
				ResponseInfo.ContentLength = _WebData.ResponseInfo.ContentLength;
				ResponseInfo.ContentEncoding = _WebData.ResponseInfo.ContentEncoding;
			}
		}



		private void DoAutoDetection(HttpRequest httpRequest)
		{
			if (httpRequest != null)
			{
				RequestInfo.RequestedQueries = Common.NameValueCollectionToString(httpRequest.QueryString);

				// When a post back event occured "httpRequest.ContentType" contains ContentType of request
				// In other cases the "httpRequest.ContentType" is empty and we shouldn't use this property
				if (!string.IsNullOrEmpty(httpRequest.ContentType))
					RequestInfo.ContentTypeString = httpRequest.ContentType;// Added again in version 3.7
				RequestInfo.InputStream = httpRequest.InputStream;
			}

			// Get posted form data string
			if (WebMethods.IsMethod(_HttpRequestMethod, WebMethods.DefaultMethods.POST))
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
				// Apply filter for ASP.NET pages
				// RequestInfo.PostedFormData = RequestInfo.PostedFormData; //Same

				if (_AutoDetection)
				{
					// Change requested url by posted data
					RequestInfo.RequestUrl = UrlBuilder.AppendAntoherQueries(RequestInfo.RequestUrl, RequestInfo.PostedFormData);
				}
			}

		}


		/// <summary>
		/// Detect request information from HttpRequest
		/// </summary>
		/// <param name="httpRequest">HttpRequest instance</param>
		private void InitializeRequestQueriesByHttpRequest(HttpRequest httpRequest)
		{
			if (_AutoDetection)
			{

				// Get http request method
				_HttpRequestMethod = httpRequest.HttpMethod;

				// If Modified Since
				// BUG: v5beta4 custom headers donsn't support at this moment
				//RequestInfo.CustomHeaders = GetHeadersFromRequest(httpRequest);
				RequestInfo.IfModifiedSince = GetIfModifiedSinceHeader(httpRequest);


				bool tmpBool = false;

				// Get requested url
				string url = httpRequest.QueryString[Consts.qUrlAddress];

				// if url is provided
				if (!string.IsNullOrEmpty(url))
				{
					string decode = httpRequest.QueryString[Consts.qDecode];
					if (decode != null)
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
					{
						url = UrlProvider.DecodeUrl(url);
					}
					RequestInfo.RequestUrl = url;
				}

				url = httpRequest.QueryString[Consts.qRedirect];
				
				if (!string.IsNullOrEmpty(url))
				{
					// If url is encoded, decode it
					if (tmpBool)
					{
						url = UrlProvider.DecodeUrl(url);
					}
					RequestInfo.RedirectedFrom = url;
				}

				// Get request post method state
				string reqMethod;
				if (UrlProvider.GetRequestQuery(httpRequest.QueryString, Consts.qWebMethod, out reqMethod))
				{
					RequestInfo.RequestMethod = WebMethods.ValidateMethod(reqMethod, WebMethods.DefaultMethods.GET);
				}
				else
					RequestInfo.RequestMethod = WebMethods.GET;
			}
		}

		#endregion
	}

	public enum ProcessTypeForData
	{
		None,
		HTML,
		JavaScript,
		CSS
	}



}