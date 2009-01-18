using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace SalarSoft.ASProxy
{
	public class WebDataCore
	{
		#region Private variables
		/// <summary>
		/// Maximum cookies size is 1 MB.
		/// </summary>
		private const int _MaxCookieSize = 1024 * 1024;
		private bool _AcceptCookies = true;
		private Exception _LastException = null;
		private LastActivityStatus _LastStatus = LastActivityStatus.Normal;
		private WebRequest _WebRequest = null;
		private WebRequestProtocolType _RequestProtocol = WebRequestProtocolType.HTTP;
		private MemoryStream _ResponseData = new MemoryStream();
		private string _ErrorMessage = "";
		private bool _DisplayErrorPageAsResult = false;
		#endregion

		#region Properties
		public DataCoreRequestInformation RequestInfo;
		public DataCoreResponseInformation ResponseInfo;
		public bool DisplayErrorPageAsResult
		{
			get { return _DisplayErrorPageAsResult; }
			set { _DisplayErrorPageAsResult = value; }
		}
		public bool AcceptCookies
		{
			get { return _AcceptCookies; }
			set { _AcceptCookies = value; }
		}
		public Exception LastException
		{
			get { return _LastException; }
			set { _LastException = value; }
		}
		public LastActivityStatus Status
		{
			get { return _LastStatus; }
			set { _LastStatus = value; }
		}
		public MemoryStream ResponseData
		{
			get { return _ResponseData; }
			set { _ResponseData = value; }
		}
		public string ErrorMessage
		{
			get { return _ErrorMessage; }
			set { _ErrorMessage = value; }
		}
		#endregion

		#region Constructors
		public WebDataCore(string url)
		{
			InitializeRequest(url, "");
		}

		public WebDataCore(string url, string userAgent)
		{
			InitializeRequest(url, userAgent);
		}

		private void InitializeRequest(string url, string userAgent)
		{

			ResponseInfo.ContentEncoding = Encoding.UTF8;

			if (_WebRequest == null)
				_WebRequest = WebRequest.Create(url);

			// Detect request protocol type
			_RequestProtocol = DetectRequestProtocol(_WebRequest);

			if (_RequestProtocol != WebRequestProtocolType.FTP)
			{
				// Set the Credentials
				_WebRequest.Credentials = CredentialCache.DefaultCredentials;
			}

			// Set default values by protocol type
			switch (_RequestProtocol)
			{
				case WebRequestProtocolType.HTTP:
					HttpWebRequest httpReq = ((HttpWebRequest)_WebRequest);

					// Enabling cookies
					if (_AcceptCookies)
					{
						httpReq.CookieContainer = new CookieContainer();
						httpReq.CookieContainer.MaxCookieSize = _MaxCookieSize;
					}

					// Does not allow auto rediraction.
					// Causes Bug; Fixed in version 4
					// BUG: In second redirection cookies does not saved
					// BUG-FIXED in V4.3
					httpReq.AllowAutoRedirect = false;

					// Use request url as referer
					switch (RequestInfo.ReferrerUsage)
					{
						case ReferrerType.ASProxySite:
							httpReq.Referer = GlobalConsts.ASProxyProjectUrl;
							break;
						case ReferrerType.RequesterAsReferrer:
							httpReq.Referer = _WebRequest.RequestUri.ToString();
							break;

						case ReferrerType.Referrer:
							httpReq.Referer = RequestInfo.Referrer;
							break;
					}

					// Initializing method
					if (string.IsNullOrEmpty(RequestInfo.RequestMethod) == false)
						httpReq.Method = RequestInfo.RequestMethod;

					if (string.IsNullOrEmpty(userAgent) == false)
						httpReq.UserAgent = userAgent;
					else
						httpReq.UserAgent = GlobalConsts.ASProxyUserAgent;

					RequestInfo.UserAgent = httpReq.UserAgent;
					break;

				case WebRequestProtocolType.FTP:
					FtpWebRequest ftpReq = ((FtpWebRequest)_WebRequest);

					ftpReq.Method = System.Net.WebRequestMethods.Ftp.DownloadFile;
					break;

				case WebRequestProtocolType.File:
					FileWebRequest fileReq = ((FileWebRequest)_WebRequest);
					break;

				case WebRequestProtocolType.Other:

					break;
			}

			// Set the request url
			RequestInfo.RequestUrl = url;
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Confirm the request and get the data from the web.
		/// </summary>
		public void Execute()
		{
			// Initialize the request
			InitializeRequest(RequestInfo.RequestUrl, RequestInfo.UserAgent);

			bool isHttpRequest = (_RequestProtocol == WebRequestProtocolType.HTTP);

			HttpWebRequest httpRequest = null;
			if (isHttpRequest)
				httpRequest = ((HttpWebRequest)_WebRequest);


			// Set execution timeout
			if (isHttpRequest)
			{
				//HttpWebRequest httpReq = ((HttpWebRequest)_WebRequest);

				// Timeout property is in milliseconds 
				httpRequest.ReadWriteTimeout = GlobalConsts.RequestFormReadWriteTimeOut;
			}

			// Timeout for request (Timeout property is in milliseconds ) 
			_WebRequest.Timeout = GlobalConsts.RequestTimeOut;

			// If configuration applied to use proxy server between ASProxy and the web.
			if (ASProxyConfig.IsWebProxyEnabled)
			{
				// Use a proxy server between ASProxy and web
				_WebRequest.Proxy = ASProxyConfig.GenerateWebProxy();
			}

			// declare response variable
			WebResponse webResponse = null;

			// declare response stream
			Stream readStream = null;

			// Start main process
			try
			{
				if (_RequestProtocol != WebRequestProtocolType.FTP)
				{
					// ADDED again in v3.7:: Set request content type
					// Bug fixed v4.8, ftp does not support this
					_WebRequest.ContentType = RequestInfo.ContentType;
				}

				// add custom headers to request
				if (RequestInfo.CustomHeaders != null)
					SetCustomHeaders(_WebRequest);

				// ASProxy signature in request header
				_WebRequest.Headers.Add("X-Powered-By", GlobalConsts.ASProxyAgentVersion);
				_WebRequest.Headers.Add("X-Working-With", GlobalConsts.ASProxyAgentVersion);

				// Add cookies to request
				if (_AcceptCookies)
				{
					// Enable the cookies
					if (isHttpRequest)
					{
						httpRequest.CookieContainer = new CookieContainer();
					}

					if (RequestInfo.Cookies == null || RequestInfo.Cookies.Count == 0)
						CookieManager.AddCookiesToRequest(_WebRequest);
					else
						CookieManager.AddCookiesToRequest(_WebRequest, RequestInfo.Cookies);
				}



				// FIXED BUG, Since V4.3:
				// In the post methods the cookies should be setted before sending post data.
				// Because the "SetRequestPostData" uses "GetRequestStream" method and
				// this sends a request. This causes a request without cookies.
				// (Damn, I was searching 3 months to find why cookies didn't send)

				// Set post data
				SetRequestPostData(_WebRequest);


				if (RequestInfo.IsCertificated)
				{
					// ADDED Since v5
					_WebRequest.Credentials = RequestInfo.GetCertification();
				}
				else if (ASProxyCredentialCache.IsCertificated(RequestInfo.RequestUrl))
				{
					// ADDED Since v4.1
					_WebRequest.Credentials = ASProxyCredentialCache.GetNetworkCertification(RequestInfo.RequestUrl);
				}

				try
				{
					// Get response from web
					webResponse = _WebRequest.GetResponse();

				}
				catch (WebException ex)
				{
					// ADDED Since V4.1:
					// Captures unauthorized access errors.
					// If the error isn't an unauthorized access error, then throw a relative exception.

					WebResponse response = ex.Response;
					if (response != null)
					{
						if (response is HttpWebResponse)
						{
							HttpWebResponse webReq = (HttpWebResponse)response;
							if (webReq.StatusCode == HttpStatusCode.Unauthorized)
							{

								// Set unauthorized response data
								SetUnauthorizedReponseData(webReq);

								this.Status = LastActivityStatus.Normal;

								// Do not continue the proccess
								return;
							}
							else throw;
						}
						else if (response is FtpWebResponse)
						{
							FtpWebResponse ftpReq = (FtpWebResponse)response;
							if (ftpReq.StatusCode == FtpStatusCode.NotLoggedIn)
							{
								// Set unauthorized response data
								SetUnauthorizedReponseData(ftpReq);

								this.Status = LastActivityStatus.Normal;

								// Do not continue the proccess
								return;
							}
							else throw;

						}
						else throw;
					}
					else throw;
				}

				if (isHttpRequest)
				{
					// Saves additional headers
					GetResponseHeaders((HttpWebResponse)webResponse);
				}


				// Set response data
				SetReponseData(webResponse);

				// Set status of request
				SetResponseHttpStatus(webResponse, ref ResponseInfo);

				this._LastStatus = LastActivityStatus.Normal;
			}
			catch (WebException webErr)
			{
				try
				{
					//ADDED 3.8.1:: Try to recover the request state and display original error state

					// Display original error page if requested.
					if (_DisplayErrorPageAsResult == false)
						throw;

					// Set response data to error statement
					SetReponseData(webErr.Response);

					// Set status of request
					SetResponseHttpStatus(webErr.Response, ref ResponseInfo);

					this._LastStatus = LastActivityStatus.NormalErrorPage;
					this.ErrorMessage = webErr.Message;
				}
				catch (Exception)
				{
					// Set status of error
					SetResponseHttpStatus(webErr.Response, ref ResponseInfo);

					this._LastStatus = LastActivityStatus.Error;
					this._ErrorMessage = webErr.Message;

					// Just for debug //this._ErrorMessage +="\n<br />"+ GetWebExceptionMessage(err.Status)  ;
					if (webErr.Status == WebExceptionStatus.ConnectFailure)
						this._ErrorMessage += "\n<br />" + "ASProxy is behind a firewall? If so, go through the proxy server or config ASProxy to pass it.";

					this.ResponseInfo.ContentLength = -1;
					this._LastException = webErr;
				}
			}
			catch (Exception err)
			{

				SetResponseHttpStatus(null, ref ResponseInfo);
				this._LastStatus = LastActivityStatus.Error;
				this._ErrorMessage = err.Message;
				this.ResponseInfo.ContentLength = -1;
				this._LastException = err;
			}
			finally
			{
				if (readStream != null)
					readStream.Close();
				if (webResponse != null)
					webResponse.Close();
			}

		}


		public void Dispose()
		{
			if (_ResponseData != null)
			{
				_ResponseData.Close();
				_ResponseData.Dispose();
			}
			_ResponseData = null;
			if (_WebRequest != null)
				_WebRequest.Abort();

			_WebRequest = null;
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Apply redirect location to request location
		/// </summary>
		private string ReplaceResponseRedirectionLocation(Uri responseUri, string redirectLocation)
		{

			if (UrlProvider.IsVirtualUrl(redirectLocation))
			{
				string pathAndQuery = responseUri.PathAndQuery;
				if (redirectLocation[0] != '/' && redirectLocation[0] != '\\')
				{
					redirectLocation = '/' + redirectLocation;

					if (pathAndQuery.Length > 1)
						return responseUri.ToString().Replace(pathAndQuery, redirectLocation);
					else
						return UrlBuilder.CombinePaths(responseUri.ToString(), redirectLocation);
				}
				else
				{
					return UrlBuilder.CombinePaths(UrlProvider.GetAppAbsolutePath(responseUri), redirectLocation);
				}
			}
			else
				return redirectLocation;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="webResponse"></param>
		/// <Added>Since V4.1</Added>
		private void SetUnauthorizedReponseData(WebResponse webResponse)
		{
			// Restore returned cookies to client
			if (_AcceptCookies)
			{
				CookieManager.RestoreCookiesFromResponse(webResponse);

				ResponseInfo.Cookies = new CookieCollection();
				CookieManager.RestoreCookiesFromResponse(webResponse, ResponseInfo.Cookies);
			}

			// Set redirect location
			ResponseInfo.AutoRedirectLocation = FilesConsts.AuthorizationPage;

			ResponseInfo.AutoRedirectionType = AutoRedirectType.ASProxyPages;
			ResponseInfo.AutoRedirect = true;

			// Get content encoding
			SetReponseContentEncoding(webResponse);

			// Get response details
			GetResponseInformation(webResponse);

			// Some correction
			_ResponseData.Capacity = Convert.ToInt32(_ResponseData.Length);
		}


		private void GetResponseHeaders(HttpWebResponse httpResponse)
		{
			if (ResponseInfo.Headers == null)
				ResponseInfo.Headers = new WebHeaderCollection();
			string temp = null;

			temp = httpResponse.Headers[HttpResponseHeader.RetryAfter];
			if (string.IsNullOrEmpty(temp) == false)
				ResponseInfo.Headers[HttpResponseHeader.RetryAfter] = temp;


			temp = httpResponse.Headers[HttpResponseHeader.Pragma];
			if (string.IsNullOrEmpty(temp) == false)
				ResponseInfo.Headers[HttpResponseHeader.Pragma] = temp;


			temp = httpResponse.Headers[HttpResponseHeader.LastModified];
			if (string.IsNullOrEmpty(temp) == false)
				ResponseInfo.Headers[HttpResponseHeader.LastModified] = temp;

			temp = httpResponse.Headers[HttpResponseHeader.LastModified];
			if (string.IsNullOrEmpty(temp) == false)
				ResponseInfo.Headers[HttpResponseHeader.LastModified] = temp;


			temp = httpResponse.Headers[HttpResponseHeader.Expires];
			if (string.IsNullOrEmpty(temp) == false)
				ResponseInfo.Headers[HttpResponseHeader.Expires] = temp;

			temp = httpResponse.Headers[HttpResponseHeader.ETag];
			if (string.IsNullOrEmpty(temp) == false)
				ResponseInfo.Headers[HttpResponseHeader.ETag] = temp;

			temp = httpResponse.Headers[HttpResponseHeader.Date];
			if (string.IsNullOrEmpty(temp) == false)
				ResponseInfo.Headers[HttpResponseHeader.Date] = temp;

			temp = httpResponse.Headers[HttpResponseHeader.CacheControl];
			if (string.IsNullOrEmpty(temp) == false)
				ResponseInfo.Headers[HttpResponseHeader.CacheControl] = temp;

			temp = httpResponse.Headers[HttpResponseHeader.Age];
			if (string.IsNullOrEmpty(temp) == false)
				ResponseInfo.Headers[HttpResponseHeader.Age] = temp;

			//temp = httpResponse.Headers["Accept-Encoding"];
			//if (string.IsNullOrEmpty(temp) == false)
			//    ResponseInfo.Headers["Accept-Encoding"] = temp;

			//temp = httpResponse.Headers["If-Modified-Since"];
			//if (string.IsNullOrEmpty(temp) == false)
			//    ResponseInfo.Headers["If-Modified-Since"] = temp;

			//temp = httpResponse.Headers["If-None-Match"];
			//if (string.IsNullOrEmpty(temp) == false)
			//    ResponseInfo.Headers["If-None-Match"] = temp;

		}

		private void SetCustomHeaders(WebRequest _WebRequest)
		{
			if (RequestInfo.CustomHeaders != null && RequestInfo.CustomHeaders.Count > 0)
			{
				NameValueCollection ajaxHeaders = RequestInfo.CustomHeaders;

				foreach (string key in ajaxHeaders)
				{
					try
					{
						_WebRequest.Headers.Add(key, ajaxHeaders[key].ToString());
					}
					catch (System.ArgumentException)
					{
						if (key.ToString().ToLower() == "content-type")
						{
							_WebRequest.ContentType = ajaxHeaders[key].ToString();
							//_WebRequest
						}
						else
							continue;
					}
					catch (Exception) { }
				}
			}
		}

		/// <summary>
		/// Applies result of request to user response data property
		/// </summary>
		/// <Added>ADDED: since version 3.8.1</Added>
		private void SetReponseData(WebResponse webResponse)
		{
			// declare response stream
			Stream readStream = null;

			// Restore returned cookies to client
			if (_AcceptCookies)
			{
				CookieManager.RestoreCookiesFromResponse(webResponse);

				ResponseInfo.Cookies = new CookieCollection();
				CookieManager.RestoreCookiesFromResponse(webResponse, ResponseInfo.Cookies);

			}

			// Redirect Location
			// ADDED Since 4.0; Ftp fixed in 4.8
			string redirectLocation = null;
			if (_RequestProtocol == WebRequestProtocolType.HTTP)
				redirectLocation = webResponse.Headers[HttpResponseHeader.Location];

			if (string.IsNullOrEmpty(redirectLocation) == false)
			{
				if (UrlProvider.IsVirtualUrl(redirectLocation))
				{
					// Get redirection full url
					redirectLocation = ReplaceResponseRedirectionLocation(webResponse.ResponseUri, redirectLocation);
					ResponseInfo.AutoRedirectionType = AutoRedirectType.RequestInternal;
				}
				else
					ResponseInfo.AutoRedirectionType = AutoRedirectType.External;

				ResponseInfo.AutoRedirectLocation = redirectLocation;
				ResponseInfo.AutoRedirect = true;
			}
			else
				ResponseInfo.AutoRedirect = false;


			// Get content encoding
			SetReponseContentEncoding(webResponse);

			// Get response details
			GetResponseInformation(webResponse);

			// If this is redirect operatin, it doesn't need to get the contents.
			if (ResponseInfo.AutoRedirect == false)
			{

				// Declare variables
				int fMaxBlockReadFromWeb = 1024;
				int contentLen = -1,
					readed = -1;
				byte[] buffer = new byte[fMaxBlockReadFromWeb];

				// Get content length as int32
				contentLen = Convert.ToInt32(webResponse.ContentLength);

				// Get the response stream for reading
				readStream = webResponse.GetResponseStream();

				// Read the stream and write it into memory
				while ((int)(readed = readStream.Read(buffer, 0, fMaxBlockReadFromWeb)) > 0)
				{
					_ResponseData.Write(buffer, 0, readed);
				}
			}

			// Sometimes the capacity is larger than length!!
			// So I correct it here
			_ResponseData.Capacity = Convert.ToInt32(_ResponseData.Length);
		}


		private void SetReponseContentEncoding(WebResponse reponse)
		{
			try
			{
				// CharacterSet is only for HTTP
				if (_RequestProtocol == WebRequestProtocolType.HTTP)
				{
					HttpWebResponse httpReponse = ((HttpWebResponse)reponse);
					ResponseInfo.ContentEncoding = Encoding.GetEncoding(httpReponse.CharacterSet);
				}
			}
			catch
			{
				// Nothing
			}
		}

		private void SetResponseHttpStatus(WebResponse response, ref DataCoreResponseInformation responseInformation)
		{
			HttpWebResponse httpResponse;
			if (response is HttpWebResponse)
			{
				httpResponse = (HttpWebResponse)response;
				responseInformation.HttpStatusDescription = httpResponse.StatusDescription;
				responseInformation.HttpStatusCode = (int)httpResponse.StatusCode;
			}
			else
			{
				responseInformation.HttpStatusDescription = "";
				responseInformation.HttpStatusCode = (int)HttpStatusCode.OK;
			}
		}

		private void GetResponseInformation(WebResponse webResponse)
		{
			// Get attached file name if available
			if (_RequestProtocol != WebRequestProtocolType.FTP)
				ResponseInfo.ContentFilename = DetectContentDispositionFile(webResponse.Headers["Content-Disposition"]);

			// Get response url address
			ResponseInfo.ResponseUrl = webResponse.ResponseUri.OriginalString;

			// Get response url base path
			ResponseInfo.SiteBasePath = webResponse.ResponseUri.Scheme
				+ "://" + webResponse.ResponseUri.Authority // .Host changed to Authority in Version 4.0
				+ "" + webResponse.ResponseUri.Segments[0];

			// Get response application path
			ResponseInfo.ContentLength = webResponse.ContentLength;

			// Get response content type
			if (_RequestProtocol != WebRequestProtocolType.FTP)
				ResponseInfo.ContentType = webResponse.ContentType;

		}

		/// <summary>
		/// Set request query information such as post data
		/// </summary>
		private void SetRequestPostData(WebRequest request)
		{
			if (WebMethods.IsMethod(RequestInfo.RequestMethod, WebMethods.DefaultMethods.POST))
			{

				if (RequestInfo.InputStream != null)
				{
					Stream reqStream = request.GetRequestStream();

					// Read stream then reset renamed ViewState name to default
					byte[] inputBytes = Common.AspDotNetViewStateResetToDef(RequestInfo.InputStream);

					// Write to destination stream
					reqStream.Write(inputBytes, 0, inputBytes.Length);
					reqStream.Close();
					RequestInfo.InputStream.Close();
				}
				else
				{
					ASCIIEncoding encoding = new ASCIIEncoding();
					string postData = Common.AspDotNetViewStateResetToDef(RequestInfo.PostDataString);

					byte[] bytes = Encoding.ASCII.GetBytes(postData);
					request.ContentLength = bytes.LongLength;

					Stream reqStream = request.GetRequestStream();
					reqStream.Write(bytes, 0, bytes.Length);
					reqStream.Close();
				}
			}
		}


		private string DetectContentDispositionFile(string str)
		{
			try
			{
				if (string.IsNullOrEmpty(str))
					return null;

				int start;
				string result = str.ToLower();
				start = result.IndexOf("filename=");
				if (start == -1)
					return null;

				start = start + "filename=".Length;
				result = str.Substring(start, str.Length - start);
				result = result.Trim();
				if (result[0] == '\'' || result[0] == '"')
					result = result.Remove(0, 1);
				if (result[result.Length - 1] == '\'' || result[result.Length - 1] == '"')
					result = result.Remove(result.Length - 1, 1);
				return result;
			}
			catch
			{
				return null;
			}
		}

		private WebRequestProtocolType DetectRequestProtocol(WebRequest _WebRequestInstance)
		{
			if (_WebRequestInstance is HttpWebRequest)
				return WebRequestProtocolType.HTTP;
			else if (_WebRequestInstance is FtpWebRequest)
				return WebRequestProtocolType.FTP;
			else if (_WebRequestInstance is FileWebRequest)
				return WebRequestProtocolType.File;
			else
				return WebRequestProtocolType.Other;
		}
		#endregion
	}

	public enum WebRequestProtocolType
	{
		HTTP,
		FTP,
		File,
		Other
	}
	public struct DataCoreRequestInformation
	{
		private string _Username;
		private string _Password;

		public DateTime IfModifiedSince;
		public string PostDataString;
		public Stream InputStream;
		public string RequestUrl;
		public string ContentType;
		public string UserAgent;
		public string Referrer;
		public string RequestMethod;
		public ReferrerType ReferrerUsage;
		public CookieCollection Cookies;
		public NameValueCollection CustomHeaders;


		public bool IsCertificated;
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
	}

	public struct DataCoreResponseInformation
	{
		public bool AutoRedirect;
		//public bool AutoRedirectIsInternal;
		public AutoRedirectType AutoRedirectionType;
		public string AutoRedirectLocation;
		public long ContentLength;
		public bool IsHttpRequest;
		public string ContentFilename;
		public string ResponseUrl;
		public string SiteBasePath;
		public string ContentType;
		public Encoding ContentEncoding;
		public string HttpStatusDescription;
		public int HttpStatusCode;
		public CookieCollection Cookies;
		public WebHeaderCollection Headers;

		public override string ToString()
		{
			return HttpStatusCode + " " + ResponseUrl;
		}
	}

}
