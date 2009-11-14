using System;
using SalarSoft.ASProxy.Exposed;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using SalarSoft.ResumableDownload;

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
		private WebResponse _webResponse;
		private InternetProtocols _requestProtocol;
		private bool _isPluginAvailable;
		#endregion

		#region properties
		#endregion

		#region public methods
		public WebData()
		{
			RequestInfo = new WebDataRequestInfo();
			ResponseInfo = new WebDataResponseInfo();
			//ResponseData = new MemoryStream();

			_requestProtocol = InternetProtocols.HTTP;

			// getting plugin availablity state
			_isPluginAvailable = Plugins.IsPluginAvailable(PluginHosts.IPluginWebData);
		}
		public override void Dispose()
		{
			if (ResponseData != null)
			{
				ResponseData.Dispose();
			}

			// make sure if response is disposed
			if (_webResponse != null)
			{
				_webResponse.Close();
				_webResponse = null;
			}

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
			// fresh
			_webResponse = null;
			try
			{

				// Create a request instance
				if (_webRequest == null)
					_webRequest = WebRequest.Create(RequestInfo.RequestUrl);

				// Initializa the instance
				InitializeWebRequest(_webRequest);

				// Post data
				ApplyPostDataToRequest(_webRequest);

				// Partial content data ranges
				ApplyContentRanges(_webRequest);

				// 0- executing plugins
				if (_isPluginAvailable)
					Plugins.CallPluginMethod(PluginHosts.IPluginWebData,
						PluginMethods.IPluginWebData.BeforeExecuteGetResponse,
						this, _webRequest);

				try
				{
					// Get the response
					_webResponse = _webRequest.GetResponse();
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
							HttpWebResponse webRes = (HttpWebResponse)response;
							if (webRes.StatusCode == HttpStatusCode.Unauthorized)
							{

								// Set unauthorized response data
								FinalizeUnauthorizedWebResponse(webRes);

								// response range
								ReadResponseRangeInfo(webRes);

								// Set status to normal
								LastStatus = LastStatus.Normal;

								// Do not continue the proccess
								return;
							}
						}
						else if (response is FtpWebResponse)
						{
							FtpWebResponse ftpRes = (FtpWebResponse)response;
							if (ftpRes.StatusCode == FtpStatusCode.NotLoggedIn)
							{
								// Set unauthorized response data
								FinalizeUnauthorizedWebResponse(ftpRes);

								// response range
								ReadResponseRangeInfo(ftpRes);

								// Set status to normal
								LastStatus = LastStatus.Normal;

								// Do not continue the proccess
								return;
							}
						}
					}

					// Nothing is captured, so throw the error
					throw;
				}

				// 1- executing plugins
				if (_isPluginAvailable)
					Plugins.CallPluginMethod(PluginHosts.IPluginWebData,
						PluginMethods.IPluginWebData.AfterExecuteGetResponse,
						this, _webResponse);


				// Check for response persmission set from "Administration UI"
				ValidateResponse(_webResponse);

				// Response is successfull, continue to get data
				FinalizeWebResponse(_webResponse);

				// response range
				ReadResponseRangeInfo(_webResponse);

				// 2- executing plugins
				if (_isPluginAvailable)
					Plugins.CallPluginMethod(PluginHosts.IPluginWebData,
						PluginMethods.IPluginWebData.AfterExecuteFinalizeWebResponse,
						this, _webResponse);

				// Getting data
				ResponseData = ReadResponseData(_webResponse);

				// 3- executing plugins
				if (_isPluginAvailable)
					Plugins.CallPluginMethod(PluginHosts.IPluginWebData,
						PluginMethods.IPluginWebData.AfterExecuteReadResponseData,
						this);
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
					LastErrorMessage = ex.Message +
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

				// response range
				ReadResponseRangeInfo(ex.Response);

				// 2- executing plugins
				if (_isPluginAvailable)
					Plugins.CallPluginMethod(PluginHosts.IPluginWebData,
						PluginMethods.IPluginWebData.AfterExecuteFinalizeWebResponse,
						this, ex.Response);

				// Getting data
				ResponseData = ReadResponseData(_webResponse);

				// 3- executing plugins
				if (_isPluginAvailable)
					Plugins.CallPluginMethod(PluginHosts.IPluginWebData,
						PluginMethods.IPluginWebData.AfterExecuteReadResponseData,
						this);


				// The state is error page
				LastStatus = LastStatus.ContinueWithError;
				LastException = ex;
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
				// dispose only if data is ready
				// not for streaming options
				if (RequestInfo.BufferResponse)
					if (_webResponse != null)
					{
						_webResponse.Close();
						_webResponse = null;
					}
			}
		}


		private void ValidateResponse(WebResponse webResponse)
		{
			// is max content lenght enabled
			if (Configurations.WebData.MaxContentLength > 0)
			{
				// is response lenght bigger than allowed size
				if (webResponse.ContentLength > Configurations.WebData.MaxContentLength)
				{
					// the request is not allowed
					string exFormat = "Getting {0} bytes of data is more than allowed size, {1} bytes.";
					throw new Exception(string.Format(exFormat, webResponse.ContentLength, Configurations.WebData.MaxContentLength));
				}
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
						if (key.ToLower() == "content-type")
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

			//temp = httpResponse.Headers[HttpResponseHeader.ETag];
			//if (string.IsNullOrEmpty(temp) == false)
			//    ResponseInfo.Headers[HttpResponseHeader.ETag] = temp;

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
						Systems.LogSystem.LogError(ex, "WebData.FinalizeUnauthorizedWebResponse", webResponse.ResponseUri.ToString());

					LastStatus = LastStatus.ContinueWithError;
					LastErrorMessage = ex.Message;
					LastException = ex;
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

			// response protocol
			ResponseInfo.ResponseProtocol = DetectWebResponseProtocol(webResponse);

		}

		/// <summary>
		/// Saves back-end response data
		/// </summary>
		/// <param name="webResponse"></param>
		protected virtual void FinalizeWebResponse(WebResponse webResponse)
		{
			if (webResponse is HttpWebResponse)
			{
				HttpWebResponse httpResponse = (webResponse as HttpWebResponse);

				// Saves additional headers
				SaveResponseHeaders(httpResponse);

				// Response charset encoding, only for HTTP
				ResponseInfo.ContentEncoding = GetResponseEncoding(httpResponse, DefaultContentEncoding);


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
						redirectLocation = ReplaceResponseRedirectionLocation(webResponse.ResponseUri,
							ResponseInfo.ResponseRootUrl,
							redirectLocation);
						ResponseInfo.AutoRedirectionType = AutoRedirectType.RequestInternal;
					}
					else
						ResponseInfo.AutoRedirectionType = AutoRedirectType.External;

					ResponseInfo.AutoRedirectLocation = redirectLocation;
					ResponseInfo.AutoRedirect = true;
				}
				else
					ResponseInfo.AutoRedirect = false;

			}
			else if (webResponse is FtpWebResponse)
			{
				FtpWebResponse ftpResponse = (webResponse as FtpWebResponse);

				// Get response url address
				ResponseInfo.ResponseUrl = ftpResponse.ResponseUri.OriginalString;

				// Get response url base path
				ResponseInfo.ResponseRootUrl = UrlProvider.GetRootPath(webResponse.ResponseUri);

				// Get response application path
				ResponseInfo.ContentLength = ftpResponse.ContentLength;
			}

			// response protocol
			ResponseInfo.ResponseProtocol = DetectWebResponseProtocol(webResponse);

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
					LastException = ex;
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
				try
				{
					// This method may cause mono to fail to load, "Need EnvironmentPermission implementation first"
					// Set the Credentials
					webRequest.Credentials = CredentialCache.DefaultCredentials;
				}
				catch
				{
					// Just simple Mono exception
				}


				// ADDED again in v3.7:: Set request content type
				// Bug fixed v4.8, ftp does not support this
				webRequest.ContentType = RequestInfo.ContentType;
			}

			// Timeout is the number of milliseconds that a subsequent 
			// synchronous request made with the GetResponse method waits
			// for a response, and the GetRequestStream method waits for a stream.
			if (RequestInfo.RequesterType == RequesterType.Download)
				webRequest.Timeout = Configurations.WebData.Downloader_Timeout;
			else
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

					// The ReadWriteTimeout property controls the time-out for the Read method,
					// which is used to read the stream returned by the GetResponseStream method,
					// and for the Write method, which is used to write to the stream returned by
					// the GetRequestStream method.
					// Timeout is in milliseconds 
					if (RequestInfo.RequesterType == RequesterType.Download)
						httpRequest.ReadWriteTimeout = Configurations.WebData.Downloader_ReadWriteTimeOut;
					else
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
							LastException = ex;
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
		/// Reads response content range info
		/// </summary>
		protected virtual void ReadResponseRangeInfo(WebResponse webResponse)
		{
			int rangeBegin;
			long rangeEnd;
			long rangeLength;
			long dataLength;
			bool rangeResponse;

			if (webResponse is HttpWebResponse)
			{
				if (!ResumableTransfers.ParseResponseHeaderRange(webResponse, out rangeBegin,
					out rangeEnd, out rangeLength, out dataLength, out rangeResponse))
				{
					// invalid headers!
					// Just ignore the partial content!
					ResponseInfo.RangeBegin = 0;
					ResponseInfo.RangeEnd = webResponse.ContentLength;
					ResponseInfo.RangeResponse = false;
					ResponseInfo.ContentLength = webResponse.ContentLength;
				}
				else
				{
					// apply to response
					ResponseInfo.RangeBegin = rangeBegin;
					ResponseInfo.RangeEnd = rangeEnd;
					ResponseInfo.RangeResponse = rangeResponse;
					ResponseInfo.ContentLength = dataLength;
				}
			}
			else
			{
				// Ftp always support resume support but, it is not possible to a proxy to know what content lenght us
				if (RequestInfo.RangeRequest)
				{
					// the requested ranges is sent by ftp
					ResponseInfo.RangeBegin = RequestInfo.RangeBegin;
					ResponseInfo.RangeEnd = RequestInfo.RangeEnd;
					ResponseInfo.ContentLength = webResponse.ContentLength;
					
					// always!
					ResponseInfo.RangeResponse = false;
				}
				else
				{
					ResponseInfo.RangeBegin = 0;
					ResponseInfo.RangeEnd = webResponse.ContentLength;
					ResponseInfo.RangeResponse = false;
					ResponseInfo.ContentLength = webResponse.ContentLength;
				}
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
		/// Detects internet protocol used
		/// </summary>
		protected virtual InternetProtocols DetectWebResponseProtocol(WebResponse webResponse)
		{
			if (webResponse is HttpWebResponse)
				return InternetProtocols.HTTP;
			else if (webResponse is FtpWebResponse)
				return InternetProtocols.FTP;
			else if (webResponse is FileWebResponse)
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
		protected virtual string ReplaceResponseRedirectionLocation(Uri responseUri, string siteRootUrl, string redirectLocation)
		{
			string pageUrl = responseUri.ToString();
			string pagePath = UrlProvider.GetPagePath(pageUrl);
			string pageUrlNoQuery = UrlProvider.GetPageAbsolutePath(pageUrl);


			if (redirectLocation[0] != '/' && redirectLocation[0] != '\\')
			{
				return UrlBuilder.CombinePaths(pagePath, redirectLocation);
			}
			else
			{
				return UrlBuilder.CombinePaths(siteRootUrl, redirectLocation);
			}
		}

		protected virtual Encoding GetResponseEncoding(HttpWebResponse httpReponse, Encoding defaultEncoding)
		{
			string enc = httpReponse.CharacterSet;
			try
			{
				if (!string.IsNullOrEmpty(enc))
					// CharacterSet is only for HTTP
					return Encoding.GetEncoding(httpReponse.CharacterSet);
				else
					return defaultEncoding;
			}
			catch
			{
				return defaultEncoding;
			}
		}


		#endregion

	}
}
