using System;
using SalarSoft.ASProxy.Exposed;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using System.IO;

namespace SalarSoft.ASProxy.BuiltIn
{
	/// <summary>
	/// Represents data communication between a request and response
	/// </summary>
	public class WebData : ExWebData
	{
		/// <summary>
		/// Maximum cookies size is 1 MB.
		/// </summary>
		private const int MaxCookieSize = 1024 * 1024;
		private static readonly Encoding DefaultContentEncoding;

		#region variables
		private WebRequest _webRequest;
		private InternetProtocols _requestProtocol;
		#endregion

		#region properties
		#endregion

		#region public methods
		public WebData()
		{
			RequestInfo = new WebDataRequestInfo();
			ResponseInfo = new WebDataResponseInfo();
			ResponseData = new MemoryStream();

			_requestProtocol = InternetProtocols.HTTP;
		}
		public override void Dispose()
		{
			if (ResponseData != null)
				ResponseData.Dispose();
		}
		#endregion

		#region static methods
		static WebData()
		{
			DefaultContentEncoding = Encoding.UTF8;
		}
		#endregion

		#region protected methods
		public override void Execute()
		{
			// response variable
			WebResponse webResponse = null;
			try
			{

				// Create a request instance
				if (_webRequest == null)
					_webRequest = WebRequest.Create(RequestInfo.RequestUrl);

				// Initializa the instance
				InitializeWebRequest(_webRequest);

				// Post data
				ApplyPostDataToRequest(_webRequest);


				try
				{
					// Get the response
					webResponse = _webRequest.GetResponse();
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
								FinalizeUnauthorizedWebResponse(webReq);

								// Set status to normal
								LastStatus = LastStatus.Normal;

								// Do not continue the proccess
								return;
							}
						}
						else if (response is FtpWebResponse)
						{
							FtpWebResponse ftpReq = (FtpWebResponse)response;
							if (ftpReq.StatusCode == FtpStatusCode.NotLoggedIn)
							{
								// Set unauthorized response data
								FinalizeUnauthorizedWebResponse(ftpReq);

								// Set status to normal
								LastStatus = LastStatus.Normal;

								// Do not continue the proccess
								return;
							}
						}
					}

					// Nothing is captured, so continue with error
					throw;
				}

				// Response is successfull, continue to get data
				FinalizeWebResponse(webResponse);

				// Getting data
				ReadResponseData(webResponse, ResponseData);

			}
			catch (WebException ex)
			{
				// adds a special message for conenction failure
				if (ex.Status == WebExceptionStatus.ConnectFailure)
				{
					// http status
					if (ex.Response != null)
					{
						ApplyResponseHttpStatus(ex.Response);
					}
					else
					{
						ResponseInfo.HttpStatusDescription = ex.Message;
						ResponseInfo.HttpStatusCode = (int)HttpStatusCode.NotFound;
					}

					ResponseInfo.ContentLength = -1;
					LastStatus = LastStatus.Error;
					LastException = ex;

					// special message
					this.LastErrorMessage = ex.Message +
						"\n<br />" + "ASProxy is behind a firewall? If so, go through the proxy server or config ASProxy to pass it.";

					return;
				}

				// ADDED 3.8.1::
				// Try to recover the request state and display original error state

				// Display original error page if requested.
				if (!RequestInfo.PrrocessErrorPage)
					throw;

				// Continue to get data
				FinalizeWebResponse(ex.Response);

				// Getting data
				ReadResponseData(webResponse, ResponseData);

				// The state is error page
				LastStatus = LastStatus.ContinueWithError;
				LastErrorMessage = ex.Message;
			}
			catch (Exception ex)
			{
				ResponseInfo.ContentLength = -1;
				LastStatus = LastStatus.Error;
				LastErrorMessage = ex.Message;
				LastException = ex;
			}
			finally
			{
				if (webResponse != null)
					webResponse.Close();
			}
		}
		protected override void ApplyCustomHeaders(WebRequest webRequest)
		{
			NameValueCollection headers = RequestInfo.CustomHeaders;
			if (headers != null && headers.Count > 0)
			{
				foreach (string key in headers)
				{
					try
					{
						webRequest.Headers.Add(key, headers[key].ToString());
					}
					catch (ArgumentException)
					{
						if (key.ToString().ToLower() == "content-type")
						{
							_webRequest.ContentType = headers[key].ToString();
						}
						else
							continue;
					}
					catch (Exception) { }
				}
			}
		}
		#endregion

		#region protected virtual methods

		protected virtual void SaveResponseHeaders(HttpWebResponse httpResponse)
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

		/// <summary>
		/// Saves back-end error response data
		/// </summary>
		/// <param name="webResponse"></param>
		protected virtual void FinalizeUnauthorizedWebResponse(WebResponse webResponse)
		{
			// Restore returned cookies to client
			if (RequestInfo.AcceptCookies)
			{
				try
				{
					Systems.CookieManager.RestoreCookiesFromResponse(webResponse, RequestInfo.TempCookies);

					ResponseInfo.Cookies = new CookieCollection();
					Systems.CookieManager.RestoreCookiesFromResponse(webResponse, ResponseInfo.Cookies);
				}
				catch (Exception ex)
				{
					if (Systems.LogSystem.ErrorLogEnabled)
						Systems.LogSystem.LogError(ex, webResponse.ResponseUri.ToString());

					LastErrorMessage = ex.Message;
					LastStatus = LastStatus.ContinueWithError;
				}
			}

			if (webResponse is HttpWebResponse)
			{
				// Response charset encoding, only for HTTP
				ResponseInfo.ContentEncoding = GetResponseEncoding((HttpWebResponse)webResponse, DefaultContentEncoding);

			}

			// Set redirect location
			ResponseInfo.AutoRedirectLocation = Consts.FilesConsts.PageAuthorization;
			ResponseInfo.AutoRedirectionType = AutoRedirectType.ASProxyPages;
			ResponseInfo.AutoRedirect = true;


			if (_requestProtocol != InternetProtocols.FTP)
			{
				// Get attached file name if available
				ResponseInfo.ContentFilename = DetectContentDispositionFile(webResponse.Headers["Content-Disposition"]);

				// Get response content type
				ResponseInfo.ContentType = webResponse.ContentType;
			}

			// Get response url address
			ResponseInfo.ResponseUrl = webResponse.ResponseUri.OriginalString;

			// Get response url base path
			ResponseInfo.ResponseRootUrl = UrlProvider.GetRootPath(webResponse.ResponseUri);

			// Get response application path
			ResponseInfo.ContentLength = webResponse.ContentLength;


		}

		/// <summary>
		/// Saves back-end response data
		/// </summary>
		/// <param name="webResponse"></param>
		protected virtual void FinalizeWebResponse(WebResponse webResponse)
		{
			if (webResponse is HttpWebResponse)
			{
				HttpWebResponse httpResponse = (HttpWebResponse)webResponse;

				// Saves additional headers
				SaveResponseHeaders(httpResponse);

				// Response charset encoding, only for HTTP
				ResponseInfo.ContentEncoding = GetResponseEncoding(httpResponse, DefaultContentEncoding);

				// Redirect Location
				// ADDED Since 4.0; Ftp fixed in 4.8
				string redirectLocation = null;

				// only in HTTP
				redirectLocation = webResponse.Headers[HttpResponseHeader.Location];

				if (!string.IsNullOrEmpty(redirectLocation))
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


				if (_requestProtocol != InternetProtocols.FTP)
				{
					// Get attached file name if available
					ResponseInfo.ContentFilename = DetectContentDispositionFile(webResponse.Headers["Content-Disposition"]);

					// Get response content type
					ResponseInfo.ContentType = webResponse.ContentType;
				}

				// Get response url address
				ResponseInfo.ResponseUrl = webResponse.ResponseUri.OriginalString;

				// Get response url base path
				ResponseInfo.ResponseRootUrl = UrlProvider.GetRootPath(webResponse.ResponseUri);

				// Get response application path
				ResponseInfo.ContentLength = webResponse.ContentLength;


			}

			// Restore returned cookies to client
			if (RequestInfo.AcceptCookies)
			{
				try
				{
					Systems.CookieManager.RestoreCookiesFromResponse(webResponse, RequestInfo.TempCookies);

					ResponseInfo.Cookies = new CookieCollection();
					Systems.CookieManager.RestoreCookiesFromResponse(webResponse, ResponseInfo.Cookies);
				}
				catch (Exception ex)
				{
					if (Systems.LogSystem.ErrorLogEnabled)
						Systems.LogSystem.LogError(ex, webResponse.ResponseUri.ToString());

					LastErrorMessage = ex.Message;
					LastStatus = LastStatus.ContinueWithError;
				}
			}

			// Response status
			ApplyResponseHttpStatus(webResponse);

		}


		/// <summary>
		/// Initializes a web request
		/// </summary>
		protected virtual void InitializeWebRequest(WebRequest webRequest)
		{

			// Detecting requested url protocol
			_requestProtocol = DetectWebRequestProtocol(webRequest);

			if (_requestProtocol != InternetProtocols.FTP)
			{
				// Set the Credentials
				webRequest.Credentials = CredentialCache.DefaultCredentials;

				// ADDED again in v3.7:: Set request content type
				// Bug fixed v4.8, ftp does not support this
				webRequest.ContentType = RequestInfo.ContentType;
			}

			// request timeout (Timeout is in milliseconds ) 
			// webRequest.Timeout = Consts.BackEndConenction.RequestTimeOut;
			webRequest.Timeout = Configurations.WebData.RequestTimeout;

			// If server has configured to pass through a proxy
			if (Configurations.NetProxy.WebProxyEnabled)
			{
				// Use configured settings
				webRequest.Proxy = Configurations.NetProxy.GenerateWebProxy();
			}

			// add custom headers to request
			if (RequestInfo.CustomHeaders != null)
				ApplyCustomHeaders(webRequest);


			// ASProxy signature in request header
			if (Configurations.WebData.SendSignature)
			{
				webRequest.Headers.Add("X-Powered-By", Consts.BackEndConenction.ASProxyAgentVersion);
				webRequest.Headers.Add("X-Working-With", Consts.BackEndConenction.ASProxyAgentVersion);
			}

			// request credentials
			if (RequestInfo.IsCertificated)
			{
				// ADDED Since v5
				webRequest.Credentials = RequestInfo.GetCertification();
			}
			else if (Systems.CredentialCache.IsCertificated(RequestInfo.RequestUrl))
			{
				// ADDED Since v4.1
				webRequest.Credentials = Systems.CredentialCache.GetNetworkCertification(RequestInfo.RequestUrl);
			}


			switch (_requestProtocol)
			{
				case InternetProtocols.HTTP:
					HttpWebRequest httpRequest = ((HttpWebRequest)_webRequest);

					// Set execution timeout
					// Timeout is in milliseconds 
					// httpRequest.ReadWriteTimeout = Consts.BackEndConenction.RequestFormReadWriteTimeOut;
					httpRequest.ReadWriteTimeout = Configurations.WebData.RequestReadWriteTimeOut;


					// Enabling cookies
					if (RequestInfo.AcceptCookies)
					{
						httpRequest.CookieContainer = new CookieContainer();
						httpRequest.CookieContainer.MaxCookieSize = MaxCookieSize;

						try
						{
							// TODO: Implement CookieManager
							if (RequestInfo.Cookies == null || RequestInfo.Cookies.Count == 0)
								Systems.CookieManager.AddCookiesToRequest(_webRequest);
							else
								Systems.CookieManager.AddCookiesToRequest(_webRequest, RequestInfo.Cookies);
						}
						catch (Exception ex)
						{
							if (Systems.LogSystem.ErrorLogEnabled)
								Systems.LogSystem.LogError(ex, _webRequest.RequestUri.ToString());

							LastErrorMessage = ex.Message;
							LastStatus = LastStatus.ContinueWithError;
						}
					}

					// Does not allow auto rediraction.
					// BUG: In second redirection cookies does not saved
					// BUG-FIXED in V4.3
					httpRequest.AllowAutoRedirect = false;

					// Request referrer
					switch (RequestInfo.ReferrerUsage)
					{
						case ReferrerType.None:
							httpRequest.Referer = String.Empty;
							break;
						case ReferrerType.ASProxySite:
							httpRequest.Referer = Consts.BackEndConenction.ASProxyProjectUrl;
							break;
						case ReferrerType.RequesterAsReferrer:
							httpRequest.Referer = _webRequest.RequestUri.ToString();
							break;
						case ReferrerType.Referrer:
							httpRequest.Referer = RequestInfo.Referrer;
							break;
					}

					// web method
					if (!string.IsNullOrEmpty(RequestInfo.RequestMethod))
						httpRequest.Method = RequestInfo.RequestMethod;

					// user agent
					if (!string.IsNullOrEmpty(RequestInfo.UserAgent))
						httpRequest.UserAgent = RequestInfo.UserAgent;



					break;
				case InternetProtocols.FTP:
					FtpWebRequest ftpReq = ((FtpWebRequest)_webRequest);

					// For now only downloading is supported
					ftpReq.Method = WebRequestMethods.Ftp.DownloadFile;
					break;

				case InternetProtocols.File:
					// Still nothing
					break;

				case InternetProtocols.Other:
					// Still nothing
					break;
				default:
					break;
			}

		}

		/// <summary>
		/// Http status
		/// </summary>
		/// <param name="response"></param>
		protected virtual void ApplyResponseHttpStatus(WebResponse response)
		{
			HttpWebResponse httpResponse;
			if (_requestProtocol == InternetProtocols.HTTP)
			{
				httpResponse = (HttpWebResponse)response;
				ResponseInfo.HttpStatusDescription = httpResponse.StatusDescription;
				ResponseInfo.HttpStatusCode = (int)httpResponse.StatusCode;
			}
			else
			{
				ResponseInfo.HttpStatusDescription = "";
				ResponseInfo.HttpStatusCode = (int)HttpStatusCode.OK;
			}
		}


		/// <summary>
		/// Detects internet protocol used
		/// </summary>
		protected virtual InternetProtocols DetectWebRequestProtocol(WebRequest webRequest)
		{
			if (webRequest is HttpWebRequest)
				return InternetProtocols.HTTP;
			else if (webRequest is FtpWebRequest)
				return InternetProtocols.FTP;
			else if (webRequest is FileWebRequest)
				return InternetProtocols.File;
			else
				return InternetProtocols.Other;
		}

		/// <summary>
		/// Detects Content disposition file name
		/// </summary>
		protected virtual string DetectContentDispositionFile(string str)
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

		/// <summary>
		/// Apply redirect location to request location
		/// </summary>
		protected virtual string ReplaceResponseRedirectionLocation(Uri responseUri, string redirectLocation)
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

		protected virtual Encoding GetResponseEncoding(HttpWebResponse httpReponse, Encoding defaultEncoding)
		{
			try
			{
				// CharacterSet is only for HTTP
				return Encoding.GetEncoding(httpReponse.CharacterSet);
			}
			catch
			{
				return defaultEncoding;
			}
		}


		#endregion

	}
}
